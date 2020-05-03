using System.Collections.Generic;

namespace ZaupUconomyEssentials
{
    internal class GroupComparer : IEqualityComparer<Group>
    {
        public bool Equals(Group x, Group y)
        {
            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.GroupID == y.GroupID && x.Salary == y.Salary;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(Group g)
        {
            //Get hash code for the Name field if it is not null.
            var hashGroupName = g.GroupID == null ? 0 : g.GroupID.GetHashCode();

            //Get hash code for the Salary field.
            var hashGroupSalary = g.Salary.GetHashCode();

            //Calculate the hash code for the product.
            return hashGroupName ^ hashGroupSalary;
        }
    }

    internal class PremiumGroupComparer : IEqualityComparer<PremiumGroup>
    {
        public bool Equals(PremiumGroup x, PremiumGroup y)
        {
            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.GroupID == y.GroupID && x.Salary == y.Salary;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(PremiumGroup g)
        {
            //Get hash code for the Name field if it is not null.
            var hashGroupName = g.GroupID == null ? 0 : g.GroupID.GetHashCode();

            //Get hash code for the Salary field.
            var hashGroupSalary = g.Salary.GetHashCode();

            //Calculate the hash code for the product.
            return hashGroupName ^ hashGroupSalary;
        }
    }
}