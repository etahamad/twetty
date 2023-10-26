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

        private async Task<User> GetUserFromUsername(string username)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        [HttpDelete("{username}")]
        public async Task<ActionResult<UserDto>> DeleteUser(string username)
        {
            var user = await GetUserFromUsername(username);

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
                ProfileImageURL = user.ProfileImageURL
            };

            return Ok(userResponse);
        }

        [HttpPut("UpdateUsername")]
        public async Task<ActionResult<string>> UpdateUsername(UpdateUsernameDto updateUsernameDto)
        {
            var username = User.Identity.Name;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }

            user.Username = updateUsernameDto.NewUsername;

            await _db.SaveChangesAsync();

            return Ok($"Username updated to {user.Username}.");
        }



        [HttpPut("Tweet/{id}")]
        public async Task<ActionResult<TweetDto>> EditTweet(int id, EditTweetDto updatedTweet)
        {
            var username = User.Identity.Name;
            var user = await GetUserFromUsername(username);

            if (user == null)
            {
                return NotFound();
            }

            var existingTweet = await _db.Tweets
                .Where(t => t.Id == id && t.UserId == user.UserId)
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
            var username = User.Identity.Name;
            var user = await GetUserFromUsername(username);

            if (user == null)
            {
                return NotFound();
            }

            var tweet = await _db.Tweets
                .Where(t => t.Id == id && t.UserId == user.UserId)
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
            var username = User.Identity.Name;
            var user = await GetUserFromUsername(username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var tweet = new Tweet
            {
                UserId = user.UserId,
                Content = tweetDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tweets.Add(tweet);
            await _db.SaveChangesAsync();

            var tweetResponse = new TweetDto
            {
                UserId = user.UserId,
                Content = tweet.Content,
                CreatedAt = tweet.CreatedAt
            };

            return Created($"/api/User/Tweet/{tweet.Id}", tweetResponse);
        }

        [HttpGet("Tweets")]
        public async Task<ActionResult<List<TweetDto>>> GetTweetsByUser()
        {
            var username = User.Identity.Name;
            var user = await GetUserFromUsername(username);

            if (user == null)
            {
                return NotFound();
            }

            var tweets = await _db.Tweets
                .Where(t => t.UserId == user.UserId)
                .Select(t => new TweetDto
                {
                    UserId = user.UserId,
                    Content = t.Content,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(tweets);
        }

        [HttpPost("Like")]
        public async Task<ActionResult<string>> LikeTweet(LikeDto likeDto)
        {
            var username = User.Identity.Name;
            var user = await GetUserFromUsername(username);
            var tweet = await _db.Tweets.FindAsync(likeDto.TweetId);

            if (user == null || tweet == null)
            {
                return NotFound("User or tweet not found.");
            }

            var existingLike = await _db.Likes
                .FirstOrDefaultAsync(l => l.UserId == user.UserId && l.TweetId == tweet.Id);

            if (existingLike != null)
            {
                return Conflict("User already liked this tweet.");
            }

            var like = new Like
            {
                UserId = user.UserId,
                TweetId = tweet.Id
            };

            _db.Likes.Add(like);
            await _db.SaveChangesAsync();

            return Ok("Tweet liked successfully.");
        }

        [HttpDelete("Unlike")]
        public async Task<ActionResult<string>> UnlikeTweet(int tweetId)
        {
            var username = User.Identity.Name;
            var user = await GetUserFromUsername(username);
            var tweet = await _db.Tweets.FindAsync(tweetId);

            if (user == null || tweet == null)
            {
                return NotFound("User or tweet not found.");
            }

            var like = await _db.Likes
                .FirstOrDefaultAsync(l => l.UserId == user.UserId && l.TweetId == tweet.Id);

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
            var followerUsername = User.Identity.Name;
            var follower = await GetUserFromUsername(followerUsername);
            var target = await _db.Users.FirstOrDefaultAsync(u => u.Username == followDto.TargetUsername);

            if (follower == null || target == null)
            {
                return NotFound("User not found.");
            }

            // Check if the follower is already following the target
            var existingFollow = await _db.Follows
                .FirstOrDefaultAsync(f => f.FollowerUserId == follower.UserId && f.TargetUserId == target.UserId);

            if (existingFollow != null)
            {
                return Conflict("User is already following this target.");
            }

            var follow = new Follow
            {
                FollowerUserId = follower.UserId,
                TargetUserId = target.UserId
            };

            _db.Follows.Add(follow);
            await _db.SaveChangesAsync();

            return Ok($"User {follower.Username} is now following {target.Username}.");
        }

        [HttpGet("Timeline")]
        public async Task<ActionResult<List<TweetResponseDto>>> GetTimeline()
        {
            var username = User.Identity.Name;
            var user = await GetUserFromUsername(username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var followingUserIds = await _db.Follows
                .Where(f => f.FollowerUserId == user.UserId)
                .Select(f => f.TargetUserId)
                .ToListAsync();

            followingUserIds.Add(user.UserId); // Include own tweets

            var tweets = await _db.Tweets
                .Where(t => followingUserIds.Contains(t.UserId))
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TweetResponseDto
                {
                    UserId = t.UserId,
                    Content = t.Content,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(tweets);
        }
    }
}
