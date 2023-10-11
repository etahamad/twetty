using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpDelete("{username}")]
        public async Task<ActionResult<UserDto>> DeleteUser(string username)
        {
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
            var existingTweet = await _db.Tweets.FindAsync(id);

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
            var tweet = await _db.Tweets.FindAsync(id);

            if (tweet == null)
            {
                return NotFound();
            }

            _db.Tweets.Remove(tweet);
            await _db.SaveChangesAsync();

            return Ok(tweet);
        }

        [HttpPost("Tweet")]
        public async Task<ActionResult<TweetDto>> CreateTweet(TweetDto tweetDto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == tweetDto.Username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var tweet = new Tweet
            {
                Username = user.Username,
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

        [HttpGet("Tweets/{username}")]
        public async Task<ActionResult<List<TweetDto>>> GetTweetsByUser(string username)
        {
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
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == likeDto.Username);
            var tweet = await _db.Tweets.FindAsync(likeDto.TweetId);

            if (user == null || tweet == null)
            {
                return NotFound("User or tweet not found.");
            }

            var existingLike = await _db.Likes.FirstOrDefaultAsync(l => l.Username == user.Username && l.TweetId == tweet.Id);

            if (existingLike != null)
            {
                return Conflict("User already liked this tweet.");
            }

            var like = new Like
            {
                Username = user.Username,
                TweetId = tweet.Id
            };

            _db.Likes.Add(like);
            await _db.SaveChangesAsync();

            return Ok("Tweet liked successfully.");
        }

        [HttpDelete("Unlike")]
        public async Task<ActionResult<string>> UnlikeTweet(string username, int tweetId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            var tweet = await _db.Tweets.FindAsync(tweetId);

            if (user == null || tweet == null)
            {
                return NotFound("User or tweet not found.");
            }

            var like = await _db.Likes.FirstOrDefaultAsync(l => l.Username == user.Username && l.TweetId == tweet.Id);

            if (like == null)
            {
                return NotFound("User has not liked this tweet.");
            }

            _db.Likes.Remove(like);
            await _db.SaveChangesAsync();

            return Ok("Tweet unliked successfully.");
        }
    }
}
