﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using twetty.Models;

namespace twetty.Models
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string Username { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set;}
        public byte[] PasswordSalt { get; set;}
        public DateTime CreatedAt { get; set; }
        public string ProfileImageURL { get; set; }

        public List<Tweet> Tweets { get; set; }
        public List<Like> Likes { get; set; }
        public List<Follow> Followers { get; set; }
        public List<Follow> Followings { get; set; }
        public List<Retweet> Retweets { get; set; }
        public List<Reply> Replies { get; set; }

    }
    public class Tweet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        
        [ForeignKey("Username")]
        public User User { get; set; }
        public List<Like> Likes { get; set; }
        public List<Retweet> Retweets { get; set; }
        public List<Reply> Replies { get; set; }
    }

    public class Like
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; }
        public int TweetId { get; set; }

        [ForeignKey("Username")]
        public User User { get; set; }

        [ForeignKey("TweetId")]
        public Tweet Tweet { get; set; }
    }

    public class Follow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FollowerUsername { get; set; }
        public string FollowingUsername { get; set; }

        [ForeignKey("FollowerUsername")]
        public User FollowerUser { get; set; }

        [ForeignKey("FollowingUsername")]
        public User FollowingUser { get; set; }
    }
    public class Retweet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; }
        public int TweetId { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("Username")]
        public User User { get; set; }

        [ForeignKey("TweetId")]
        public Tweet Tweet { get; set; }
    }



    public class Reply
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; }
        public int TweetId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("Username")]
        public User User { get; set; }

        [ForeignKey("TweetId")]
        public Tweet Tweet { get; set; }
    }
}