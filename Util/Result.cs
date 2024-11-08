using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Util
{
    // Result<T, E> implementation from the Rust programming language.
    // See https://doc.rust-lang.org/std/result/ for more information.
    public class Result<T, E>
    {
        public T Value { get; set; }
        public E Error { get; set; }

        public bool IsValue()
        {
            return Error == null;
        }

        public bool IsErr()
        {
            return Error != null;
        }

        public static Result<T, E> Ok(T value)
        {
            return new Result<T, E> { Value = value };
        }

        public static Result<T, E> Err(E error)
        {
            return new Result<T, E> { Error = error };
        }
    }

}
