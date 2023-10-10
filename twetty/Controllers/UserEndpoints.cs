using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;
using twetty.Context;
using twetty.Models;
using twetty.DTOs;
using System.Drawing.Text;
using System.Security.Cryptography;

namespace twetty.Controllers;

public static class UserEndpoints
{
    public static void MapUserEndpoints (this IEndpointRouteBuilder routes)
    {
        // Endpoint to delete a user by username
        routes.MapDelete("/api/Users/{username}", async (string username, ApplicationDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return Results.NotFound();
            }

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            var userResponse = new UserDto
            {
                Username = user.Username,
                Email = user.Email,
                ProfileImageURL = user.ProfileImageURL,
                CreatedAt = user.CreatedAt
            };

            return Results.Ok(userResponse);
        })
        .WithName("DeleteUser")
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        routes.MapPut("/api/Tweets/{id}", async (int id, EditTweetDto updatedTweet, ApplicationDbContext db) =>
        {
            var existingTweet = await db.Tweets.FindAsync(id);

            if (existingTweet == null)
            {
                return Results.NotFound();
            }

            existingTweet.Content = updatedTweet.Content;

            await db.SaveChangesAsync();

            var tweetResponse = new TweetDto
            {
                Content = existingTweet.Content
            };

            return Results.Ok(tweetResponse);
        })
        .WithName("EditTweet")
        .Produces<TweetDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);



        routes.MapDelete("/api/Tweets/{id}", async (int id, ApplicationDbContext db) =>
        {
            var tweet = await db.Tweets.FindAsync(id);

            if (tweet == null)
            {
                return Results.NotFound();
            }

            db.Tweets.Remove(tweet);
            await db.SaveChangesAsync();

            return Results.Ok(tweet);
        })
        .WithName("DeleteTweet")
        .Produces<TweetDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        routes.MapPost("/api/Tweets", async (TweetDto tweetDto, ApplicationDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == tweetDto.Username);

            if (user == null)
            {
                return Results.NotFound("User not found.");
            }

            var tweet = new Tweet
            {
                Username = user.Username,
                Content = tweetDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            db.Tweets.Add(tweet);
            await db.SaveChangesAsync();

            var tweetResponse = new TweetDto
            {
                Username = tweet.Username,
                Content = tweet.Content,
                CreatedAt = tweet.CreatedAt
            };

            return Results.Created($"/api/Tweets/{tweet.Id}", tweetResponse);
        })
        .WithName("CreateTweet")
        .Produces<TweetDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound);

        routes.MapGet("/api/Tweets/ByUser/{username}", async (string username, ApplicationDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return Results.NotFound();
            }

            var tweets = await db.Tweets
            .Where(t => t.Username == user.Username)
                .Select(t => new TweetDto
                {
                    Username = user.Username,
                    Content = t.Content,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Results.Ok(tweets);
        })
        .WithName("GetTweetsByUser")
        .Produces<List<TweetDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        routes.MapPost("/api/Likes", async (LikeDto likeDto, ApplicationDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == likeDto.Username);
            var tweet = await db.Tweets.FindAsync(likeDto.TweetId);

            if (user == null || tweet == null)
            {
                return Results.NotFound("User or tweet not found.");
            }

            var existingLike = await db.Likes.FirstOrDefaultAsync(l => l.Username == user.Username && l.TweetId == tweet.Id);

            if (existingLike != null)
            {
                return Results.Conflict("User already liked this tweet.");
            }

            var like = new Like
            {
                Username = user.Username,
                TweetId = tweet.Id
            };

            db.Likes.Add(like);
            await db.SaveChangesAsync();

            return Results.Ok("Tweet liked successfully.");
        })
        .WithName("LikeTweet")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        routes.MapDelete("/api/Likes", async (string username, int tweetId, ApplicationDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            var tweet = await db.Tweets.FindAsync(tweetId);

            if (user == null || tweet == null)
            {
                return Results.NotFound("User or tweet not found.");
            }

            var like = await db.Likes.FirstOrDefaultAsync(l => l.Username == user.Username && l.TweetId == tweet.Id);

            if (like == null)
            {
                return Results.NotFound("User has not liked this tweet.");
            }

            db.Likes.Remove(like);
            await db.SaveChangesAsync();

            return Results.Ok("Tweet unliked successfully.");
        })
        .WithName("UnlikeTweet")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status404NotFound);


    }
}
