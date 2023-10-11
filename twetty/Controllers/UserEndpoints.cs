using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using twetty.Context;
using twetty.DTOs;
using twetty.Models;

namespace twetty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string GetUsernameFromClaims()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value;
        }

        [HttpDelete]
        public async Task<ActionResult<UserDto>> DeleteUser()
        {
            var username = GetUsernameFromClaims();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            var userResponse = new UserDto
            {
                Username = user.Username,
                Email = user.Email,
                ProfileImageURL = user.ProfileImageURL,
                CreatedAt = user.CreatedAt
            };

            return Ok(userResponse);
        }

        [HttpPut("Tweet/{id}")]
        public async Task<ActionResult<TweetDto>> EditTweet(int id, EditTweetDto updatedTweet)
        {
            var username = GetUsernameFromClaims();

            var existingTweet = await _db.Tweets
                .Where(t => t.Id == id && t.Username == username)
                .FirstOrDefaultAsync();

            if (existingTweet == null)
            {
                return NotFound();
            }

            existingTweet.Content = updatedTweet.Content;

            await _db.SaveChangesAsync();

            var tweetResponse = new TweetDto
            {
                Content = existingTweet.Content
            };

            return Ok(tweetResponse);
        }

        [HttpDelete("Tweet/{id}")]
        public async Task<ActionResult<Tweet>> DeleteTweet(int id)
        {
            var username = GetUsernameFromClaims();

            var tweet = await _db.Tweets
                .Where(t => t.Id == id && t.Username == username)
                .FirstOrDefaultAsync();

            if (tweet == null)
            {
                return NotFound();
            }

            _db.Tweets.Remove(tweet);
            await _db.SaveChangesAsync();

            return Ok(tweet);
        }

        [HttpPost("Tweet")]
        public async Task<ActionResult<TweetDto>> CreateTweet(CreateTweetDto tweetDto)
        {
            var username = GetUsernameFromClaims();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var tweet = new Tweet
            {
                Username = username,
                Content = tweetDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tweets.Add(tweet);
            await _db.SaveChangesAsync();

            var tweetResponse = new TweetDto
            {
                Username = tweet.Username,
                Content = tweet.Content,
                CreatedAt = tweet.CreatedAt
            };

            return Created($"/api/Tweets/{tweet.Id}", tweetResponse);
        }



        [HttpGet("Tweets")]
        public async Task<ActionResult<List<TweetDto>>> GetTweetsByUser()
        {
            var username = GetUsernameFromClaims();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }

            var tweets = await _db.Tweets
                .Where(t => t.Username == user.Username)
                .Select(t => new TweetDto
                {
                    Username = user.Username,
                    Content = t.Content,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(tweets);
        }

        [HttpPost("Like")]
        public async Task<ActionResult<string>> LikeTweet(LikeDto likeDto)
        {
            var username = GetUsernameFromClaims();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            var tweet = await _db.Tweets.FindAsync(likeDto.TweetId);

            if (user == null || tweet == null)
            {
                return NotFound("User or tweet not found.");
            }

            var existingLike = await _db.Likes
                .FirstOrDefaultAsync(l => l.Username == username && l.TweetId == tweet.Id);

            if (existingLike != null)
            {
                return Conflict("User already liked this tweet.");
            }

            var like = new Like
            {
                Username = username,
                TweetId = tweet.Id
            };

            _db.Likes.Add(like);
            await _db.SaveChangesAsync();

            return Ok("Tweet liked successfully.");
        }

        [HttpDelete("Unlike")]
        public async Task<ActionResult<string>> UnlikeTweet(int tweetId)
        {
            var username = GetUsernameFromClaims();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            var tweet = await _db.Tweets.FindAsync(tweetId);

            if (user == null || tweet == null)
            {
                return NotFound("User or tweet not found.");
            }

            var like = await _db.Likes
                .FirstOrDefaultAsync(l => l.Username == username && l.TweetId == tweet.Id);

            if (like == null)
            {
                return NotFound("User has not liked this tweet.");
            }

            _db.Likes.Remove(like);
            await _db.SaveChangesAsync();

            return Ok("Tweet unliked successfully.");
        }
        [HttpPost("Follow")]
        public async Task<ActionResult<string>> FollowUser(FollowDto followDto)
        {
            var followerUsername = GetUsernameFromClaims();
            var targetUsername = followDto.TargetUsername;

            var follower = await _db.Users.FirstOrDefaultAsync(u => u.Username == followerUsername);
            var target = await _db.Users.FirstOrDefaultAsync(u => u.Username == targetUsername);

            if (follower == null || target == null)
            {
                return NotFound("User not found.");
            }

            // Check if the follower is already following the target
            var existingFollow = await _db.Follows
                .FirstOrDefaultAsync(f => f.FollowerUsername == followerUsername && f.TargetUsername == targetUsername);

            if (existingFollow != null)
            {
                return Conflict("User is already following this target.");
            }

            var follow = new Follow
            {
                FollowerUsername = followerUsername,
                TargetUsername = targetUsername
            };

            _db.Follows.Add(follow);
            await _db.SaveChangesAsync();

            return Ok($"User {followerUsername} is now following {targetUsername}.");
        }

        [HttpGet("Timeline")]
        public async Task<ActionResult<List<TweetResponseDto>>> GetTimeline()
        {
            var username = GetUsernameFromClaims();

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var followingUsernames = await _db.Follows
                .Where(f => f.FollowerUsername == username)
                .Select(f => f.TargetUsername)
                .ToListAsync();

            followingUsernames.Add(username); // Include own tweets

            var tweets = await _db.Tweets
                .Where(t => followingUsernames.Contains(t.Username))
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TweetResponseDto
                {
                    Username = t.Username,
                    Content = t.Content,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(tweets);
        }

    }
}
