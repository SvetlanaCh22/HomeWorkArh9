using bookrepository;
using domainbook;

namespace bookmanager {

    public interface IBookService
    {
        void addBook(String title, String author, double price);
        void removeBook(String title, String author, double price);
        List<Book> getAllBooks();
    }

    public class Business : IBookService
    {
        private readonly BookRepository bookRepository;
            
        public Business(BookRepository bookRepository) {
            this.bookRepository = bookRepository;
        }

        public void addBook(String title, String author, double price) {
            Book book = new Book(title, author, price);
            bookRepository.addBook(book);
        }

        public void removeBook(String title, String author, double price) {
            Book book = new Book(title, author, price);
            bookRepository.removeBook(book);
        }

        public void removeBook(Int32 id)
        {
            bookRepository.removeBook(id);
        }

        public List<Book> getAllBooks() {
            return bookRepository.getAllBooks();
        }
    }
}