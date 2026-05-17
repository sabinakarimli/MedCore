namespace HospitalApp.Models
{
    using System.Text.RegularExpressions;

    public abstract class Person
    {
        // ── Static ID Generator ─────────────────────────────────────────────────
        private static int _nextId = 1000;
        private static readonly Regex _phoneRegex = new(@"^\+?[\d\s\-\(\)]{7,20}$", RegexOptions.Compiled);
        private static readonly Regex _nameRegex = new(@".*[a-zA-Z].*", RegexOptions.Compiled);

        // ── Encapsulated Fields ─────────────────────────────────────────────────
        private string _phone = string.Empty;
        private int    _age;

        // ── Public Properties ───────────────────────────────────────────────────
        public int      Id           { get; }
        public string   FullName     { get; protected set; }
        public string   Email        { get; protected set; }
        public DateTime RegisteredAt { get; }

        public string Phone
        {
            get => _phone;
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Phone number cannot be empty.");
                if (!_phoneRegex.IsMatch(value.Trim()))
                    throw new ArgumentException("Phone number format is invalid. Use digits, spaces, dashes, parentheses, or leading '+'.");
                _phone = value.Trim();
            }
        }

        public int Age
        {
            get => _age;
            protected set
            {
                if (value < 0 || value > 150)
                    throw new ArgumentOutOfRangeException(nameof(Age), "Age must be between 0 and 150.");
                _age = value;
            }
        }

        // ── Abstract Members (subclasses must implement) ────────────────────────
        public abstract string Role        { get; }
        public abstract string RoleIcon    { get; }
        public abstract string GetDetails();

        // ── Constructor ─────────────────────────────────────────────────────────
        protected Person(string fullName, int age, string phone, string email = "")
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
            if (!_nameRegex.IsMatch(fullName))
                throw new ArgumentException("Full name must contain at least one letter.", nameof(fullName));
            if (!string.IsNullOrEmpty(email) && !email.Contains('@'))
                throw new ArgumentException("Invalid email format.", nameof(email));

            Id           = _nextId++;
            FullName     = fullName.Trim();
            Age          = age;
            Phone        = phone;
            Email        = email;
            RegisteredAt = DateTime.Now;
        }

        // ── Virtual Methods (can be overridden) ─────────────────────────────────
        public virtual string GetInfo() =>
            $"  {RoleIcon} #{Id:D4}  {FullName,-24} Age:{Age,3}  {Phone,-16}  [{Role}]";

        public override string ToString() => GetInfo();
    }
}
