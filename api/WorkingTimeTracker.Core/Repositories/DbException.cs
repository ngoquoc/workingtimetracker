using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingTimeTracker.Core.Repositories
{
    public class RepositoryException : Exception
    {
        public RepositoryException() { }
        public RepositoryException(string message) : base(message) { }
        public RepositoryException(string message, Exception inner) : base(message, inner) { }
    }

    public class RelationshipException : RepositoryException
    {
        public RelationshipException() { }
        public RelationshipException(string message) : base(message) { }
        public RelationshipException(string message, Exception inner) : base(message, inner) { }
    }
}
