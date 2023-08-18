using bookmanager;
using bookrepository;
using domainbook;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Web;
using static System.Reflection.Metadata.BlobBuilder;

namespace HomeWorkArh9
{

    internal static class Program
    {

        public static Business? bookService;
        public static HttpListener server = new HttpListener();
        public static int pageViews = 0;
        public static int requestCount = 0;

        static void Main(String[] args)
        {
            BookRepository bookRepository = new BookRepository();
            bookService = new Business(bookRepository);

            while (true)
            {
                Console.WriteLine("1 - добавить книгу");
                Console.WriteLine("2 - удалить книгу");
                Console.WriteLine("3 - показать все книги");
                Console.WriteLine("4 - запуск сервера");
                Console.WriteLine("5 - выйти из программы");
                Console.Write("Выберите раздел меню: ");

                int choice = Convert.ToInt32(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        addBook();
                        break;
                    case 2:
                        removeBook();
                        break;
                    case 3:
                        showAllBooks();
                        break;
                    case 4:
                        StartServer();
                        break;
                    case 5:
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Выбран несуществующий пунк меню");
                        break;
                }
            }
        }

        static void StartServer()
        {
            // установка адресов прослушки
            server.Prefixes.Add("http://127.0.0.1:8888/");
            // начинаем прослушивать входящие подключения
            server.Start(); 

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            Console.WriteLine("Сервер запущен на порту 8888");
            Console.WriteLine("Запросы обрабытваются асинхронно");
        }

        static void StopServer()
        {
            server.Stop(); // останавливаем сервер
            server.Close(); // закрываем HttpListener
        }

        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;
            string title;
            string author;
            double price;

            string responseString;
            byte[] buffer = Encoding.UTF8.GetBytes("");

            BookRepository bookRepository = new BookRepository();
            bookService = new Business(bookRepository);

            while (runServer)
            {
                HttpListenerContext ctx = await server.GetContextAsync();
                HttpListenerRequest request = ctx.Request;
                HttpListenerResponse response = ctx.Response;

                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(request.Url.ToString());
                Console.WriteLine(request.HttpMethod);
                Console.WriteLine(request.UserHostName);
                Console.WriteLine(request.UserAgent);
                Console.WriteLine();

                Uri myUri = new Uri(request.Url.AbsolutePath);

                // получить список всех книг
                if ((request.HttpMethod == "GET") && (request.Url.AbsolutePath == "/books"))
                {
                    List<Book> books = bookService.getAllBooks();
                    responseString = JsonSerializer.Serialize(books);
                    buffer = Encoding.UTF8.GetBytes(responseString);

                }
                // добавить новую книгу или обновить информацию, если указан id
                else if ((request.HttpMethod == "POST") && (request.Url.AbsolutePath.StartsWith("/books")))
                {
                    title = HttpUtility.ParseQueryString(myUri.Query).Get("title");
                    author = HttpUtility.ParseQueryString(myUri.Query).Get("author");
                    price = Convert.ToDouble(HttpUtility.ParseQueryString(myUri.Query).Get("price"));

                    String id_str = HttpUtility.ParseQueryString(myUri.Query).Get("id");

                    // если id есть - обновляем
                    if (id_str != null)
                    {
                        Int32 id = Convert.ToInt32(HttpUtility.ParseQueryString(myUri.Query).Get("id"));
                        List<Book> books = bookService.getAllBooks();
                        if (books.Count < id)
                        {
                            Console.WriteLine("Книга не найдена");
                        }
                        else
                        {
                            books[id].setTitle(title);
                            books[id].setAuthor(author);
                            books[id].setPrice(price);
                            Console.WriteLine("Данные книги обновлены");
                        }
                    }
                    // если id нет - добавлояем новую книгу
                    else
                    {
                        bookService.addBook(title, author, price);
                        Console.WriteLine("Книга добавлена в базу данных");
                    }

                    runServer = false;
                }
                // получить информацию о конкретной книге по ID
                else if ((request.HttpMethod == "GET") && (request.Url.AbsolutePath.StartsWith("/books")))
                {
                    String id_str = HttpUtility.ParseQueryString(myUri.Query).Get("id");

                    if (id_str != null)
                    {
                        Int32 id = Convert.ToInt32(id_str);

                        List<Book> books = bookService.getAllBooks();

                        if (books.Count == 0)
                        {
                            Console.WriteLine("Книги не найдены");
                        }
                        else
                        {
                            responseString = JsonSerializer.Serialize(books[id]);
                            buffer = Encoding.UTF8.GetBytes(responseString);
                        }
                    }
                }
                // удалить книгу
                else if ((request.HttpMethod == "DELETE") && (request.Url.AbsolutePath == "/books"))
                {
                    String id_str = HttpUtility.ParseQueryString(myUri.Query).Get("id");

                    if (id_str != null)
                    {
                        Int32 id = Convert.ToInt32(id_str);

                        bookService.removeBook(id);

                        Console.WriteLine("Книга удалена");
                    }
                    else
                    {
                        Console.WriteLine("Книга не найдена");
                    }
                }

                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = buffer.Length;

                runServer = false;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
        }

        private static void addBook()
        {
            String title = ""; String author = ""; double price = 0;
            askforBook(ref title, ref author, ref price);

            bookService.addBook(title, author, price);

            Console.WriteLine("Книга добавлена в базу данных");
        }


        private static void removeBook()
        {
            String title = ""; String author = ""; double price = 0;
            askforBook(ref title, ref author, ref price);

            bookService.removeBook(title, author, price);

            Console.WriteLine("Книга удалена из базы данных");
        }

        private static void askforBook(ref String title, ref String author, ref double price)
        {
            Console.Write("Введите название книжки: ");
            title = Convert.ToString(Console.ReadLine());

            Console.Write("Введите автора: ");
            author = Convert.ToString(Console.ReadLine());

            Console.Write("Введите цену: ");
            price = Convert.ToDouble(Console.ReadLine());
        }

        private static void showAllBooks()
        {
            List<Book> books = bookService.getAllBooks();

            if (books.Count == 0)
            {
                Console.WriteLine("Книги не найдены");
            }
            else
            {
                foreach (Book book in books)
                {
                    Console.WriteLine(book.getTitle() + " автора " + book.getAuthor() + " - цена: " + book.getPrice());
                    Console.WriteLine("");
                }
            }
        }
    }
}