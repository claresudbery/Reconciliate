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
                new Person { FirstName = "Jean" },
                new Person { FirstName = "Jack" },
                new Person { FirstName = "Jill" },
                new Person { FirstName = "Jiminy" }
            };

        [Test]
        public void CanUseLinqToFindAllLettersWhichAreRepeatedAcrossSeveralNames()
        {
            // !! This was originally created for the following repo:
            // https://github.com/claresudbery/linq-exercises
            // ... and has in fact been copied over there, into CombinedOperationExperiment.cs. 

            List<List<char>> names = _people
                .Select(person => person.FirstName.ToList())
                .ToList();

            var result = names
                .Skip(1)
                .Aggregate(
                    new Thing1
                    {
                        PreviousElements = new List<List<char>> {names[0]},
                        CharactersInCommon = new List<char>()
                    },
                    (resultSoFar, nextElement) =>
                        new Thing1()
                        {
                            PreviousElements = resultSoFar.PreviousElements.Union(new List<List<char>> {nextElement}).ToList(),
                            CharactersInCommon = resultSoFar.PreviousElements
                                .Aggregate(
                                    nextElement.Intersect(resultSoFar.PreviousElements[0])
                                        .ToList(),
                                    (backResultSoFar, nextBackElement) => 
                                    backResultSoFar.Union(
                                        nextElement
                                        .Intersect(nextBackElement))
                                        .ToList())
                                .Union(resultSoFar.CharactersInCommon)
                                .ToList()
                        }).CharactersInCommon;

            Assert.IsTrue(result.OrderBy(x => x).SequenceEqual(new char[] { 'a', 'i', 'J', 'n' }.OrderBy(x => x)));
        }
    }

    public class Thing2
    {
        public List<char> CurrentElement { get; set; }
        public List<char> BackCharactersInCommon { get; set; }
    }

    public class Thing1
    {
        public List<List<char>> PreviousElements { get; set; }
        public List<char> CharactersInCommon { get; set; }
    }

    public class Person
    {
        public string FirstName { get; set; }
    }
}
