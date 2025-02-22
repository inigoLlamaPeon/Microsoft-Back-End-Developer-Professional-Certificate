using System;
using System.Collections.Generic;

class Program
{
    const int MaxBooks = 5;
    const int MaximumCopies = 3;

    class Book
    {
        public string Title { get; set; } = "";
        public int Copies { get; set; }
        public int LoanedCopies { get; set; }

    }

    static void Main(string[] args)
    {
        List<Book> bookList = new List<Book>();

        int option = 0;
        Console.WriteLine("Welcome to the Book Manager");

        while (true)
        {
            Console.WriteLine("Select an option:");
            Console.WriteLine("1- Search Book (exact title)");
            Console.WriteLine("2- Search Book (partial title)");
            Console.WriteLine("3- Add New Title");
            Console.WriteLine("4- Remove Book From Library");
            Console.WriteLine("5- Loan Book");
            Console.WriteLine("6- Return Book");
            Console.WriteLine("7- List Books");
            Console.WriteLine("8- Quit");
            if (!int.TryParse(Console.ReadLine(), out option))
            {
                Console.WriteLine("Invalid option!!! Please enter a valid integer.");
                continue;
            }
            switch (option)
            {
                case 1: // Search book by exact title
                    SearchBook(bookList);
                    break;

                case 2: // Search book by partial title
                    SearchByPartialTitle(bookList);
                    break;

                case 3: // Add new title to the library
                    AddNewTitle(bookList);
                    break;

                case 4: // Remove book from library
                    ListBooks(bookList);
                    RemoveBook(bookList);
                    break;

                case 5: // Loan book
                    LoanBook(bookList);
                    break;

                case 6: // Return book
                    ReturnBook(bookList);
                    break;

                case 7:// List books
                    ListBooks(bookList);
                    break;

                case 8: // Quit
                    Quit();
                    return;

                default:
                    Console.WriteLine("Invalid option!!!");
                    break;
            }
            Console.WriteLine("--------------------");
        }
    }

    static void SearchBook(List<Book> bookList)
    {
        Console.WriteLine("Enter the title of the book you want to search:");
        string title = Console.ReadLine().ToLower();
        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("Title cannot be empty!");
            return;
        }
        Book book = bookList.Find(b => b.Title == title);
        if (book != null)
        {
            Console.WriteLine($"Title: {book.Title}, Copies: {book.Copies}, Loaned Copies: {book.LoanedCopies}");
        }
        else
        {
            Console.WriteLine("Book not found!");
        }
    }

    static void SearchByPartialTitle(List<Book> bookList)
    {
        Console.WriteLine("Enter the partial title of the book you want to search:");
        string title = Console.ReadLine().ToLower();
        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("Title cannot be empty!");
            return;
        }
        List<Book> books = bookList.FindAll(b => b.Title.Contains(title));
        if (books.Count > 0)
        {
            foreach (var book in books)
            {
                Console.WriteLine($"Title: {book.Title}, Copies: {book.Copies}, Loaned Copies: {book.LoanedCopies}");
            }
        }
        else
        {
            Console.WriteLine("Book not found!");
        }
    }

    static void AddNewTitle(List<Book> bookList)
    {
        if (bookList.Count >= MaxBooks)
        {
            Console.WriteLine("The library is full. You can't add more books.");
            return;
        }
        Console.WriteLine("Enter the title of the book:");
        string title = Console.ReadLine().ToLower();
        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("Title cannot be empty!");
            return;
        }
        if (bookList.Exists(b => b.Title == title))
        {
            Console.WriteLine("The book already exists in the library.");
            return;
        }

        Console.WriteLine("Enter the number of copies:");
        if (!int.TryParse(Console.ReadLine(), out int copies))
        {
            Console.WriteLine("Invalid input! Please enter a valid integer.");
            return;
        }
        if (copies <= 0 || copies > MaximumCopies)
        {
            Console.WriteLine($"Invalid number of copies! Please enter a value between 1 and {MaximumCopies}.");
            return;
        }

        Book book = new Book();
        book.Title = title;
        book.Copies = copies;
        bookList.Add(book);
        Console.WriteLine("Book added successfully.");
    }

    static void RemoveBook(List<Book> bookList)
    {
        Console.WriteLine("Enter the title of the book you want to remove:");
        string title = Console.ReadLine().ToLower();
        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("Title cannot be empty!");
            return;
        }
        Book book = bookList.Find(b => b.Title == title);
        if (book != null)
        {
            bookList.Remove(book);
            Console.WriteLine("Book removed successfully.");
        }
        else
        {
            Console.WriteLine("Book not found!");
        }
    }

    static void LoanBook(List<Book> bookList)
    {
        Console.WriteLine("Enter the title of the book you want to loan:");
        string title = Console.ReadLine().ToLower();
        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("Title cannot be empty!");
            return;
        }
        Book book = bookList.Find(b => b.Title == title);
        if (book != null)
        {
            if (book.Copies - book.LoanedCopies > 0)
            {
                book.LoanedCopies++;
                Console.WriteLine("Book loaned successfully.");
            }
            else
            {
                Console.WriteLine("No copies available for loan.");
            }
        }
        else
        {
            Console.WriteLine("Book not found!");
        }
    }

    static void ReturnBook(List<Book> bookList)
    {
        Console.WriteLine("Enter the title of the book you want to return:");
        string title = Console.ReadLine().ToLower();
        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("Title cannot be empty!");
            return;
        }
        Book book = bookList.Find(b => b.Title == title);
        if (book != null)
        {
            if (book.LoanedCopies > 0)
            {
                book.LoanedCopies--;
                Console.WriteLine("Book returned successfully.");
            }
            else
            {
                Console.WriteLine("No copies are loaned.");
            }
        }
        else
        {
            Console.WriteLine("Book not found!");
        }
    }

    static void ListBooks(List<Book> bookList)
    {
        if (bookList.Count == 0)
        {
            Console.WriteLine("No books in the library.");
            return;
        }
        foreach (var book in bookList)
        {
            Console.WriteLine($"Title: {book.Title}, Copies: {book.Copies}, Loaned Copies: {book.LoanedCopies}");
        }
    }

    static void Quit()
    {
        Console.WriteLine("Goodbye!");
    }

}