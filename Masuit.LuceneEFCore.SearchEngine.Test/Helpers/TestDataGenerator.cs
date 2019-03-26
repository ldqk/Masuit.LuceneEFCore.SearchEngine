using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.LuceneEFCore.SearchEngine.Test.Models;
using System.Collections.Generic;
using System.IO;

namespace Masuit.LuceneEFCore.SearchEngine.Test.Helpers
{
    public class TestDataGenerator
    {
        private static List<User> allTestUsers;
        private static List<City> allTestCities;

        public TestDataGenerator()
        {
            if (allTestUsers == null)
            {
                allTestUsers = new List<User>();
                TextReader reader = new StreamReader("Helpers\\TestData\\MOCK_USERS.csv");//网上下载的用户模拟数据

                string data = reader.ReadLine();

                while ((data = reader.ReadLine()) != null)
                {
                    string[] line = data.Split(',');
                    allTestUsers.Add(new User()
                    {
                        Id = int.Parse(line[0]),
                        FirstName = line[1],
                        Surname = line[2],
                        Email = line[3],
                        JobTitle = line[5]
                    });
                }

                reader.Close();
            }

            if (allTestCities == null)
            {
                allTestCities = new List<City>();
                TextReader reader = new StreamReader("Helpers\\TestData\\MOCK_CITIES.csv");

                string data = reader.ReadLine();
                while ((data = reader.ReadLine()) != null)
                {
                    string[] line = data.Split(',');
                    allTestCities.Add(new City()
                    {
                        Id = int.Parse(line[0]),
                        Country = line[1],
                        Code = line[2],
                        Name = line[3],
                    });
                }

                reader.Close();
            }
        }

        public List<ILuceneIndexable> AllData
        {
            get
            {
                List<ILuceneIndexable> data = new List<ILuceneIndexable>();
                data.AddRange(allTestUsers);
                data.AddRange(allTestCities);
                return data;
            }
        }

        public User ANewUser(string firstName = "Joe", string surname = "Bloggs", string jobTitle = "IT Consultant")
        {
            string email = firstName + "." + surname + "@test.com";
            var newUser = new User()
            {
                FirstName = firstName,
                Surname = surname,
                Email = email,
                Id = allTestUsers.Count + 1,
                JobTitle = jobTitle
            };
            return newUser;
        }
    }
}