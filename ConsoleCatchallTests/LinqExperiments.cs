using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ConsoleCatchallTests
{
    [TestFixture]
    public class LinqExperiments
    {
        private List<Person> _people = new List<Person>
            {
                new Person { First_name = "Jean" },
                new Person { First_name = "Jack" },
                new Person { First_name = "Jill" },
                new Person { First_name = "Jiminy" }
            };

        [Test]
        public void Can_use_linq_to_find_all_letters_which_are_repeated_across_several_names()
        {
            // !! This was originally created for the following repo:
            // https://github.com/claresudbery/linq-exercises
            // ... and has in fact been copied over there, into CombinedOperationExperiment.cs. 

            List<List<char>> names = _people
                .Select(person => person.First_name.ToList())
                .ToList();

            var result = names
                .Skip(1)
                .Aggregate(
                    new Thing1
                    {
                        Previous_elements = new List<List<char>> {names[0]},
                        Characters_in_common = new List<char>()
                    },
                    (result_so_far, next_element) =>
                        new Thing1()
                        {
                            Previous_elements = result_so_far.Previous_elements.Union(new List<List<char>> {next_element}).ToList(),
                            Characters_in_common = result_so_far.Previous_elements
                                .Aggregate(
                                    next_element.Intersect(result_so_far.Previous_elements[0])
                                        .ToList(),
                                    (back_result_so_far, next_back_element) => 
                                    back_result_so_far.Union(
                                        next_element
                                        .Intersect(next_back_element))
                                        .ToList())
                                .Union(result_so_far.Characters_in_common)
                                .ToList()
                        }).Characters_in_common;

            Assert.IsTrue(result.OrderBy(x => x).SequenceEqual(new char[] { 'a', 'i', 'J', 'n' }.OrderBy(x => x)));
        }
    }

    public class Thing2
    {
        public List<char> Current_element { get; set; }
        public List<char> Back_characters_in_common { get; set; }
    }

    public class Thing1
    {
        public List<List<char>> Previous_elements { get; set; }
        public List<char> Characters_in_common { get; set; }
    }

    public class Person
    {
        public string First_name { get; set; }
    }
}
