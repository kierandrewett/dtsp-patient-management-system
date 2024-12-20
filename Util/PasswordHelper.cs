﻿using System.Security.Cryptography;
using System.Text;

namespace PMS.Util
{
    internal class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            byte[] passwordByteArr = Encoding.UTF8.GetBytes(password.ToCharArray());
            byte[] hashByteArr = SHA256.HashData(passwordByteArr);

            StringBuilder stringBuilder = new();

            foreach (byte b in hashByteArr)
            {
                stringBuilder.Append(b.ToString("X2"));
            }

            return stringBuilder.ToString().ToLower();
        }

        public static string GeneratePassword()
        {
            Random rng = new();
            int animalIdx = rng.Next(0, ListOfAnimals.Count);
            int adjectiveIdx = rng.Next(0, ListOfAdjectives.Count);
            int randomInt = rng.Next(10, 99);

            string adjective = ListOfAdjectives[adjectiveIdx];
            string animal = ListOfAnimals[animalIdx];

            return $"{adjective}{animal}{randomInt}";
        }

        // Derived from https://github.com/skjorrface/animals.txt/blob/master/animals.txt
        //
        // Licensed under the MIT License
        // 
        // Copyright(c) 2018 skjorrface
        // 
        // Permission is hereby granted, free of charge, to any person obtaining a copy
        // of this software and associated documentation files (the "Software"), to deal
        // in the Software without restriction, including without limitation the rights
        // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        // copies of the Software, and to permit persons to whom the Software is
        // furnished to do so, subject to the following conditions:
        //
        // The above copyright notice and this permission notice shall be included in all
        // copies or substantial portions of the Software.
        //
        // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        // SOFTWARE.
        public static readonly List<string> ListOfAnimals = [
            "Aardvark",
            "Albatross",
            "Alligator",
            "Angelfish",
            "Ant",
            "Anteater",
            "Antelope",
            "Armadillo",
            "Baboon",
            "Badger",
            "Bandicoot",
            "Barracuda",
            "Bat",
            "Beagle",
            "Bear",
            "Beaver",
            "Bee",
            "Bird",
            "Bison",
            "Bobcat",
            "Bonobo",
            "Buffalo",
            "Bulldog",
            "Butterfly",
            "Cat",
            "Caterpillar",
            "Catfish",
            "Chameleon",
            "Cheetah",
            "Chicken",
            "Chimpanzee",
            "Coati",
            "Coyote",
            "Crab",
            "Crocodile",
            "Deer",
            "Dog",
            "Dolphin",
            "Duck",
            "Eagle",
            "Elephant",
            "Falcon",
            "Ferret",
            "Fish",
            "Flamingo",
            "Frog",
            "Giraffe",
            "Goat",
            "Goose",
            "Gopher",
            "Gorilla",
            "Hamster",
            "Hare",
            "Hawk",
            "Hedgehog",
            "Horse",
            "Human",
            "Iguana",
            "Impala",
            "Jaguar",
            "Kangaroo",
            "Koala",
            "Lion",
            "Lizard",
            "Lynx",
            "Macaw",
            "Moose",
            "Mouse",
            "Otter",
            "Ox",
            "Panda",
            "Parrot",
            "Penguin",
            "Pig",
            "Pigeon",
            "Rabbit",
            "Rat",
            "Seal",
            "Shark",
            "Sheep",
            "Shrimp",
            "Snail",
            "Snake",
            "Spider",
            "Squirrel",
            "Swan",
            "Tiger",
            "Toucan",
            "Turkey",
            "Turtle",
            "Whale",
            "Wolf",
            "Yak",
            "Zebra",
            "Zorse"
        ];

        // Derived from https://gist.github.com/hugsy/8910dc78d208e40de42deb29e62df913
        public static readonly List<string> ListOfAdjectives = [
            "Abandoned",
            "Able",
            "Absolute",
            "Adorable",
            "Aged",
            "Agile",
            "Alive",
            "Amazing",
            "Ample",
            "Ancient",
            "Angry",
            "Any",
            "Arctic",
            "Awesome",
            "Awful",
            "Babyish",
            "Bad",
            "Bare",
            "Basic",
            "Beautiful",
            "Big",
            "Bitter",
            "Black",
            "Blank",
            "Blue",
            "Brave",
            "Bright",
            "Broken",
            "Brown",
            "Bumpy",
            "Calm",
            "Cheap",
            "Clean",
            "Clear",
            "Clever",
            "Cold",
            "Common",
            "Cool",
            "Crazy",
            "Creepy",
            "Cute",
            "Dark",
            "Deadly",
            "Dear",
            "Deep",
            "Dense",
            "Dry",
            "Dull",
            "Early",
            "Easy",
            "Edible",
            "Elastic",
            "Elegant",
            "Empty",
            "Evil",
            "Faint",
            "Fair",
            "False",
            "Famous",
            "Fast",
            "Fat",
            "Fearful",
            "Fearless",
            "Female",
            "Few",
            "Fine",
            "Firm",
            "First",
            "Flat",
            "Fluffy",
            "Fresh",
            "Friendly",
            "Funny",
            "Gentle",
            "Giant",
            "Gigantic",
            "Good",
            "Grand",
            "Gray",
            "Great",
            "Green",
            "Happy",
            "Hard",
            "Heavy",
            "Helpful",
            "Helpless",
            "High",
            "Huge",
            "Hungry",
            "Icy",
            "Ill",
            "Important",
            "Innocent",
            "Intense",
            "Jagged",
            "Juicy",
            "Kind",
            "Large",
            "Last",
            "Lazy",
            "Light",
            "Little",
            "Live",
            "Long",
            "Loud",
            "Lovely",
            "Loyal",
            "Lucky",
            "Mean",
            "Messy",
            "Mild",
            "Modern",
            "Moist",
            "Narrow",
            "Nasty",
            "Natural",
            "Neat",
            "New",
            "Nice",
            "Noisy",
            "Odd",
            "Old",
            "Open",
            "Orange",
            "Pale",
            "Perfect",
            "Plain",
            "Plump",
            "Polite",
            "Poor",
            "Pretty",
            "Quick",
            "Quiet",
            "Rare",
            "Real",
            "Red",
            "Rich",
            "Rigid",
            "Rough",
            "Round",
            "Rude",
            "Safe",
            "Salty",
            "Same",
            "Scary",
            "Sharp",
            "Short",
            "Shy",
            "Simple",
            "Skinny",
            "Slim",
            "Slow",
            "Small",
            "Smooth",
            "Soft",
            "Solid",
            "Sticky",
            "Strong",
            "Sweet",
            "Tall",
            "Tame",
            "Thick",
            "Thin",
            "Tiny",
            "Tough",
            "Ugly",
            "Unfair",
            "Warm",
            "Weak",
            "Weird",
            "Wet",
            "White",
            "Wide",
            "Wild",
            "Wise",
            "Wonderful",
            "Yellow",
            "Young",
            "Yummy"

            ];
    }
}
