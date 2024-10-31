using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace MushroomPocket
{
    public class MushroomMaster
    {
        public string Name { get; set; }
        public int NoToTransform { get; set; }
        public string TransformTo { get; set; }

        public MushroomMaster(string name, int noToTransform, string transformTo)
        {
            this.Name = name;
            this.NoToTransform = noToTransform;
            this.TransformTo = transformTo;
        }
    }

    public class Character
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int hp { get; set; }
        public int exp { get; set; }
        public string skill { get; set; }
        public int atk { get; set; }

        public Character(int Id, string name, int hp, int exp, string skill, int atk)
        {
            this.Id = Id;
            this.Name = name;
            this.hp = hp;
            this.exp = exp;
            this.skill = skill;
            this.atk = atk;

        }
    }

    public class Daisy : Character
    {
        public Daisy(int Id, string name, int hp, int exp, string skill) : base(Id, name, hp, exp, skill, 15)
        {
        }
    }

    public class Wario : Character
    {
        public Wario(int Id, string name, int hp, int exp, string skill) : base(Id, name, hp, exp, skill, 20)
        {
        }
    }

    public class Waluigi : Character
    {
        public Waluigi(int Id, string name, int hp, int exp, string skill) : base(Id, name, hp, exp, skill, 10)
        {
        }
    }

    public class Peach : Character
    {
        public Peach(int Id, string name, int hp, int exp, string skill) : base(Id, name, hp, exp, skill, 25)
        {
        }
    }

    public class Mario : Character
    {
        public Mario(int Id, string name, int hp, int exp, string skill) : base(Id, name, hp, exp, skill, 30)
        {
        }
    }

    public class Luigi : Character
    {
        public Luigi(int Id, string name, int hp, int exp, string skill) : base(Id, name, hp, exp, skill, 20)
        {
        }
    }
    // Code from line 85 to 95 refrenced from ChatGpt
    public class MushroomPocketContext : DbContext
    {
        public MushroomPocketContext(DbContextOptions<MushroomPocketContext> options) : base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Character>()
            .HasKey(c => c.Id);
        }
        public DbSet<Character> Characters { get; set; }
    }
    
}