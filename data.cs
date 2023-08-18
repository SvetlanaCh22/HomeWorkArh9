using domainbook;

namespace bookrepository
{
    interface IBookRepository
    {
        void addBook(Book book);
        void removeBook(Book book);
        List<Book> getAllBooks();
    }

    public class BookRepository : IBookRepository {
        private readonly List<Book> books;

        public BookRepository() {
            this.books = new List<Book>();
        }

        public void addBook(Book book) {
            books.Add(book);
        }

        public void removeBook(Book book) {
            books.Remove(book);
        }

        public void removeBook(Int32 id)
        {
            books.RemoveAt(id);
        }

        public List<Book> getAllBooks() {
            return new List<Book>(books);
        }
    }
}