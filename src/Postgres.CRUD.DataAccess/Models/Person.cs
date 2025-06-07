namespace Postgres.CRUD.DataAccess.Models
{
  /// <summary>
  /// Entity type hierarchy mapping
  /// By convention, EF will not automatically scan for base or derived types;
  /// this means that if you want a CLR type in your hierarchy to be mapped,
  /// you must explicitly specify that type on your model.For example,
  /// specifying only the base type of a hierarchy will not cause EF Core to implicitly include all of its sub-types.
  /// </summary>
  public class Person
  {
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
  }

  public class Instructor : Person
  {
    public string Title { get; set; }
    public string Department { get; set; }
    public DateTime DateHired { get; set; }
  }

  public class Student : Person
  {
    public string Major { get; set; }
    public double GPA { get; set; }
    public DateTime DateEnrolled { get; set; }
  }

  public class Course
  {
    public string CourseName { get; set; }
    public string CourseCode { get; set; }
    public int Credits { get; set; }
    public Instructor Instructor { get; set; }
    public ICollection<Student> Students { get; set; }
  }
}
