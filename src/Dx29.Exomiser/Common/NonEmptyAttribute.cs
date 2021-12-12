using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Dx29.Exomiser
{
    public class NonEmptyAttribute : ValidationAttribute
    {
        public NonEmptyAttribute() : base("Value cannot be empty.")
        {
        }

        public override bool IsValid(object value)
        {
            var list = value as IList;
            if (list != null)
            {
                return list.Count > 0;
            }
            return false;
        }
    }
}
