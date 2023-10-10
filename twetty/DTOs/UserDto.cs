﻿using System.ComponentModel.DataAnnotations.Schema;

namespace twetty.DTOs
{
    public class UserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ProfileImageURL { get; set; }
    }

}