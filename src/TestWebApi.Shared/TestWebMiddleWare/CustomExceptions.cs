namespace TestWebApi.Shared.TestWebMiddleWare
{
    /// <summary>
    /// Base exception for this project. Keeps things simple and domain-focused.
    /// </summary>
    public abstract class ProjectException : Exception
    {
        public string Code { get; }
        protected ProjectException(string message, string code, Exception? inner = null)
            : base(message, inner)
        {
            Code = code;
        }
    }

    /// <summary>
    /// Thrown when a requested entity is not found.
    /// </summary>
    public class EntityNotFoundException : ProjectException
    {
        public string Entity { get; }
        public object? Key { get; }

        public EntityNotFoundException(string entity, object? key = null)
            : base(key is null
                ? $"{entity} was not found."
                : $"{entity} with id '{key}' was not found.",
                "NOT_FOUND")
        {
            Entity = entity;
            Key = key;
        }
    }

    public sealed class ProductNotFoundException : EntityNotFoundException
    {
        public ProductNotFoundException(object key) : base("Product", key) { }
    }

    public sealed class CategoryNotFoundException : EntityNotFoundException
    {
        public CategoryNotFoundException(object key) : base("Category", key) { }
    }

    /// <summary>
    /// Thrown when validation fails. Carries field-level errors.
    /// </summary>
    public class ValidationException : ProjectException
    {
        public IReadOnlyCollection<ValidationError> Errors { get; }

        public ValidationException(IEnumerable<ValidationError> errors)
            : base("Validation failed.", "VALIDATION_ERROR")
        {
            Errors = errors.ToArray();
        }

        public ValidationException(string field, string message)
            : this(new[] { new ValidationError(field, message) }) { }
    }

    /// <summary>
    /// Represents a single validation error.
    /// (Kept here to avoid coupling to DTO layer if needed elsewhere.)
    /// </summary>
    public readonly record struct ValidationError(string Field, string Message);

    /// <summary>
    /// Thrown when attempting to create a category with a name that already exists.
    /// </summary>
    public class DuplicateCategoryNameException : ProjectException
    {
        public string Name { get; }
        public DuplicateCategoryNameException(string name)
            : base($"Category name '{name}' already exists.", "CATEGORY_DUPLICATE_NAME")
        {
            Name = name;
        }
    }

    /// <summary>
    /// Thrown when attempting to create a product with a name that already exists (if business rule requires uniqueness).
    /// </summary>
    public class DuplicateProductNameException : ProjectException
    {
        public string Name { get; }
        public DuplicateProductNameException(string name)
            : base($"Product name '{name}' already exists.", "PRODUCT_DUPLICATE_NAME")
        {
            Name = name;
        }
    }

    /// <summary>
    /// Thrown when a category with existing products is attempted to be deleted.
    /// </summary>
    public class CategoryDeleteNotAllowedException : ProjectException
    {
        public Guid CategoryId { get; }
        public int ProductCount { get; }

        public CategoryDeleteNotAllowedException(Guid categoryId, int productCount)
            : base($"Category '{categoryId}' cannot be deleted because it still has {productCount} product(s).",
                   "CATEGORY_DELETE_NOT_ALLOWED")
        {
            CategoryId = categoryId;
            ProductCount = productCount;
        }
    }

    /// <summary>
    /// Thrown when pagination parameters exceed available bounds.
    /// </summary>
    public class PaginationOutOfRangeException : ProjectException
    {
        public int RequestedPage { get; }
        public int TotalPages { get; }

        public PaginationOutOfRangeException(int requestedPage, int totalPages)
            : base($"Requested page '{requestedPage}' is out of range. Total pages: {totalPages}.", "PAGINATION_OUT_OF_RANGE")
        {
            RequestedPage = requestedPage;
            TotalPages = totalPages;
        }
    }

    /// <summary>
    /// Thrown when a repository operation fails unexpectedly (e.g., persistence error).
    /// Wrap underlying exception when you want to shield internals.
    /// </summary>
    public class RepositoryOperationException : ProjectException
    {
        public string Operation { get; }
        public string Entity { get; }

        public RepositoryOperationException(string operation, string entity, Exception inner)
            : base($"Repository operation '{operation}' failed for entity '{entity}'.", "REPOSITORY_OPERATION_FAILED", inner)
        {
            Operation = operation;
            Entity = entity;
        }
    }
}