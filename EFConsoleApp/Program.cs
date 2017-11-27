using ClubModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Migrations;
using System.Reflection;
using System.IO;
using CsvHelper;

namespace EFConsoleApp
{



    class Program
    {
        static void Main(string[] args)
        {
            using (ClubContext context = new ClubContext())
            {
                //SeedClub(context);
                //SeedStudents(context);
                SeedCourses(context);
            }
        }

        // This is debuggable
        private static void SeedClub(ClubContext context)
        {
            #region club 1
            context.Clubs.AddOrUpdate(c => c.ClubName,

            new Club
            {
                //clubMembers=getMembers(context),
                ClubName = "The Tiddly Winks Club",
                CreationDate = DateTime.Now,
                adminID = -1, // Choosing a negative to define unassigned as all members will have a positive id later
                // It seem you cannot reliably assign the result of a method to a field while using 
                // Add Or Update. My suspicion is that it cannot evaluate whether 
                // or not it is an update. There could also be a EF version issue
                // The club events assignment will work though as it is 
                clubEvents = new List<ClubEvent>()
            {	// Create a new ClubEvent 
                        new ClubEvent { StartDateTime = DateTime.Now.Subtract( new TimeSpan(5,0,0,0,0)),
                           EndDateTime = DateTime.Now.Subtract( new TimeSpan(5,0,0,0,0)),
                           Location="Sligo", Venue="Arena",
                           // Update attendees with a method similar to the SeedClubMembers 
                           // See below
                        },
                        new ClubEvent { StartDateTime = DateTime.Now.Subtract( new TimeSpan(3,0,0,0,0)),
                           EndDateTime = DateTime.Now.Subtract( new TimeSpan(3,0,0,0,0)),
                           Location="Sligo", Venue="Main Canteen"
        },
            }
            });
            #endregion
            #region club 2
            context.Clubs.AddOrUpdate(c => c.ClubName,
            new Club
            {
                ClubName = "The Chess Club",
                CreationDate = DateTime.Now,
                adminID = -1,
                clubEvents = new List<ClubEvent>()
                {	// Create a new ClubEvent 
                        new ClubEvent { StartDateTime = DateTime.Now.Subtract( new TimeSpan(5,0,0,0,0)),
                           EndDateTime = DateTime.Now.Subtract( new TimeSpan(6,0,0,0,0)),
                           Location="Sligo", Venue="The Leitrim Bar",
                           // Update attendees with a method similar to the SeedClubMembers 
                           // See below
                        },
                        new ClubEvent { StartDateTime = DateTime.Now.Subtract( new TimeSpan(3,0,0,0,0)),
                           EndDateTime = DateTime.Now.Subtract( new TimeSpan(2,0,0,0,0)),
                           Location="Sligo", Venue="Main Canteen"
                        },
            }
            });
            #endregion
            context.SaveChanges();
        }

        public static void SeedStudents(ClubContext context)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            // You'll need to change this reference in the Club MVC app
            // as it has a different namespace
            string resourceName = "EFConsoleApp.Migrations.TestStudents.csv";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    CsvReader csvReader = new CsvReader(reader);
                    csvReader.Configuration.HasHeaderRecord = false;
                    var testStudents = csvReader.GetRecords<Student>().ToArray();
                    context.Students.AddOrUpdate(s => s.StudentID, testStudents);
                }
            }
            context.SaveChanges();
        }
        public static void SeedCourses(ClubContext context)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            // You'll need to change this reference in the Club MVC app
            // as it has a different namespace
            string resourceName = "EFConsoleApp.Migrations.Courses.csv";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    CsvReader csvReader = new CsvReader(reader);
                    csvReader.Configuration.HasHeaderRecord = false;
                    var courseData = csvReader.GetRecords<CourseDataImport>().ToArray();
                    foreach (var dataItem in courseData)
                    {
                        context.Courses.AddOrUpdate(c =>
                                new { c.CourseCode, c.CourseName }, 
                                new Course { CourseCode = dataItem.CourseCode,
                                              CourseName = dataItem.CourseName,
                                                 Year = dataItem.Year});

                    }
                }
            }
            context.SaveChanges();
        }
        public static List<Member> getMembers(ClubContext context )
        {
            var ret = GetStudents(context).Select(s => new Member { StudentID = s.StudentID }).ToList();
            return ret;
        }

        public static List<Student> GetStudents(ClubContext context)
        {
            // Create a random list of student ids
            var randomSetStudent = context.Students.Select(s => new { s.StudentID, r = Guid.NewGuid() });
            // sort them and take 10
            List<string> subset = randomSetStudent.OrderBy(s => s.r)
                .Select(s => s.StudentID.ToString()).Take(10).ToList();
            // return the selected students as a relaized list
            return context.Students.Where(s => subset.Contains(s.StudentID)).ToList();
        }
    }
}
