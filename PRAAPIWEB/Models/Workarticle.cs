﻿namespace PRAAPIWEB.Models
{
    public class Workarticle
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public string Category { get; set; } = null!;
    }
}
