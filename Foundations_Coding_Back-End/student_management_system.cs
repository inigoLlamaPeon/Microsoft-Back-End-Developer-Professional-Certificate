using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Metrics;
using System.Dynamic;


enum Options
{
    AddStudent = 1,
    ModifyGrade,
    DisplayStudentInfo,
    DisplayAllInfo,
    RemoveStudentInfo,
    RemoveSubject,
    ExitApplication

}

public class CONSTANTS
{
    public const int MAX_STUDENTS = 1024;
}

public class Student
{
    public string full_name { get; set; } = "";
    public int id { get; set; }
    public double average { get; set; }
    public Dictionary<string, double> subjects { get; set; } = new Dictionary<string, double>();

    public void update_average()
    {
        double total = 0;
        foreach (var subject in this.subjects)
        {
            total += subject.Value;
        }
        this.average = total / this.subjects.Count;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        int option = 0, students_registered = 0;
        List<Student> student_list = new List<Student>();

        Console.WriteLine("---------------------------");
        Console.WriteLine("| Student Management Tool |");
        Console.WriteLine("---------------------------");

        while (option != (int)Options.ExitApplication)
        {
            try
            {
                Console.WriteLine("Select an option:");
                Console.WriteLine("1 - Add student");
                Console.WriteLine("2 - Add / Modify student grade");
                Console.WriteLine("3 - Display student information");
                Console.WriteLine("4 - List all students information");
                Console.WriteLine("5 - Remove Student");
                Console.WriteLine("6 - Remove Subject");
                Console.WriteLine("7 - Exit");
                option = Convert.ToInt16(Console.ReadLine());
                switch (option)
                {
                    case (int)Options.AddStudent:
                        if (students_registered < CONSTANTS.MAX_STUDENTS)
                        {
                            add_student(student_list);
                            students_registered++;
                        }
                        else
                        {
                            Console.WriteLine("Maximum number of students reached. Please remove one or more students before adding new ones");
                        }
                        break;
                    case (int)Options.ModifyGrade:
                        if (students_registered > 0)
                        {
                            modify_grade(student_list);
                        }
                        else
                        {
                            Console.WriteLine("No students registered.");
                        }
                        break;
                    case (int)Options.DisplayStudentInfo:
                        if (students_registered > 0)
                        {
                            display_student_info(student_list);
                        }
                        else
                        {
                            Console.WriteLine("No students registered.");
                        }
                        break;
                    case (int)Options.DisplayAllInfo:
                        if (students_registered > 0)
                        {
                            display_all_students(student_list);
                        }
                        else
                        {
                            Console.WriteLine("No students registered.");
                        }
                        break;
                    case (int)Options.RemoveStudentInfo:
                        if (students_registered > 0)
                        {
                            remove_student(student_list);
                            students_registered--;
                        }
                        else
                        {
                            Console.WriteLine("No students registered.");
                        }
                        break;

                    case (int)Options.RemoveSubject:
                        if (students_registered > 0)
                        {
                            remove_subject(student_list);
                        }
                        else
                        {
                            Console.WriteLine("No students registered.");
                        }
                        break;

                    case (int)Options.ExitApplication:
                        Console.WriteLine("Exit application.");
                        break;
                    default:
                        Console.WriteLine("Invalid option");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error!!! {ex.Message}");
            }
            Console.WriteLine("--------------------------------");
        }
    }

    public static void add_student(List<Student> student_list)
    {
        int id;
        string full_name;
        Dictionary<string, double> subjects = new Dictionary<string, double>();

        full_name = set_string("full name");
        id = generate_id(student_list);
        subjects = add_subjects();
        Student tmp_student = new Student() { full_name = full_name, id = id, subjects = subjects };
        tmp_student.update_average();
        student_list.Add(tmp_student);
        Console.WriteLine("Student added.");
        Console.WriteLine($"Full name: {tmp_student.full_name}");
        Console.WriteLine($"ID: {tmp_student.id}");
        foreach (var elem in tmp_student.subjects)
        {
            Console.WriteLine($"{elem.Key} : {elem.Value}");
        }
        Console.WriteLine($"Average: {tmp_student.average}");

    }

    public static string set_string(string parameter)
    {
        string name = "";
        do
        {
            Console.WriteLine($"Insert {parameter}: ");
            name = Console.ReadLine();
        } while (name.Length < 1);
        return name;
    }

    public static int generate_id(List<Student> student_list)
    {
        bool valid_id = false;
        int id = 0;
        Random rnd = new Random();
        do
        {
            id = rnd.Next(1, CONSTANTS.MAX_STUDENTS + 1);
            if (student_list.Count > 0)
            {
                foreach (Student student in student_list)
                {
                    if (student.id == id)
                    {
                        valid_id = false;
                        break;
                    }
                    else
                    {
                        valid_id = true;
                    }
                }
            }
            else
            {
                valid_id = true;
            }
        } while (valid_id != true);
        return id;
    }

    public static Dictionary<string, double> add_subjects()
    {
        Dictionary<string, double> subjects = new Dictionary<string, double>();
        string subj = "";
        string grd = "";
        double grade;
        bool more_subjects = true;
        do
        {
            Console.WriteLine("Insert subject (press enter to leave):");
            subj = Console.ReadLine();
            if (subj.Length == 0)
            {
                more_subjects = false;
                break;
            }
            else
            {
                Console.WriteLine($"Insert {subj} grade:");
                grd = Console.ReadLine();
                try
                {
                    if (grd.Length > 0)
                    {
                        grade = Convert.ToDouble(grd);
                    }
                    else
                    {
                        Console.WriteLine("No value assigned. Zero is set by default");
                        grade = 0;
                    }
                    if (grade < 0 || grade > 10)
                    {
                        throw new Exception("Grade must be between 0 and 10");
                    }
                    else{
                        subjects.Add(subj.ToLower(), grade);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error!!! {e.Message}");
                }
            }
        } while (more_subjects != false);
        return subjects;
    }

    public static void modify_grade(List<Student> student_list)
    {
        bool student_found = false;
        int counter = 1;
        Console.WriteLine("Registered students:");
        foreach (Student std in student_list)
        {
            Console.WriteLine($"ID: {std.id}\tFull Name: {std.full_name}");
        }
        Console.WriteLine("Insert ID:");
        try
        {
            int request_id = Convert.ToInt32(Console.ReadLine());
            foreach (Student std in student_list)
            {
                if (std.id == request_id)
                {
                    Console.WriteLine("Available subjects");
                    foreach (var subject in std.subjects)
                    {
                        Console.WriteLine($"{counter} - {subject.Key}");
                        counter++;
                    }
                    Console.WriteLine("Insert subject name");
                    string subj = Console.ReadLine().ToLower();
                    if (subj.Length > 0)
                    {
                        Console.WriteLine($"Insert {subj} grade:");
                        double new_grade = Convert.ToDouble(Console.ReadLine());
                        if (std.subjects.ContainsKey(subj))
                        {
                            std.subjects[subj] = new_grade;
                        }
                        else
                        {
                            std.subjects.Add(subj, new_grade);
                        }
                        std.update_average();
                    }
                    student_found = true;
                }
            }
            if (student_found == false)
            {
                Console.WriteLine($"Student with ID {request_id} not found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void display_student_info(List<Student> student_list)
    {
        bool student_found = false;
        int counter = 1;
        Console.WriteLine("Registered students:");
        foreach (Student std in student_list)
        {
            Console.WriteLine($"ID: {std.id}\t\tFull Name: {std.full_name}");
        }
        Console.WriteLine("Insert ID:");
        try
        {
            int request_id = Convert.ToInt32(Console.ReadLine());
            foreach (Student std in student_list)
            {
                if (std.id == request_id)
                {
                    Console.WriteLine("--------------------");
                    Console.WriteLine($"Full name: {std.full_name}");
                    Console.WriteLine($"ID: {std.id}");
                    foreach (var elem in std.subjects)
                    {
                        Console.WriteLine($"{counter}\t{elem.Key}\t\t{elem.Value}");
                        counter++;
                    }
                    Console.WriteLine($"Average:  {std.average}");
                    student_found = true;
                }
            }
            if (student_found == false)
            {
                Console.WriteLine($"Student with ID {request_id} not found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void display_all_students(List<Student> student_list)
    {
        foreach (Student std in student_list)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine($"Full name: {std.full_name}");
            Console.WriteLine($"ID: {std.id}");
            foreach (var elem in std.subjects)
            {
                Console.WriteLine($"{elem.Key} : {elem.Value}");
            }
            Console.WriteLine($"Average:  {std.average}");
        }
    }

    public static void remove_student(List<Student> student_list)
    {
        bool deleted = false;
        Console.WriteLine("Registered students:");
        foreach (Student std in student_list)
        {
            Console.WriteLine($"ID: {std.id}\t\tFull Name: {std.full_name}");
        }
        Console.WriteLine("Insert ID:");
        try
        {
            int request_id = Convert.ToInt32(Console.ReadLine());
            foreach (Student std in student_list)
            {
                if (request_id == std.id)
                {
                    student_list.Remove(std);
                    Console.WriteLine("Student deleted");
                    deleted = true;
                    break;
                }
            }
            if (deleted == false)
            {
                Console.WriteLine("ID not found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void remove_subject(List<Student> student_list)
    {
        bool student_found = false;
        int counter = 1;
        Console.WriteLine("Registered students:");
        foreach (Student std in student_list)
        {
            Console.WriteLine($"ID: {std.id}\tFull Name: {std.full_name}");
        }
        Console.WriteLine("Insert ID:");
        try
        {
            int request_id = Convert.ToInt32(Console.ReadLine());
            foreach (Student std in student_list)
            {
                if (std.id == request_id)
                {
                    Console.WriteLine("Available subjects");
                    foreach (var subject in std.subjects)
                    {
                        Console.WriteLine($"{counter} - {subject.Key}");
                        counter++;
                    }
                    Console.WriteLine("Insert subject name");
                    string subj = Console.ReadLine().ToLower();
                    if (subj.Length > 0)
                    {
                        if (std.subjects.ContainsKey(subj))
                        {   
                            Console.WriteLine("Subject removed");
                            std.subjects.Remove(subj);
                        }
                        else{
                            Console.WriteLine("Subject not found");
                        }
                        std.update_average();
                    }
                    student_found = true;
                }
            }
            if (student_found == false)
            {
                Console.WriteLine($"Student with ID {request_id} not found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
