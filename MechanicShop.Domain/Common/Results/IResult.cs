using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicShop.Domain.Common.Results
{
    public interface IResult
    {
        List <Error> errors { get; }
        bool IsSuccess { get; }
    }
    public interface IResult<out TValue> : IResult
    {
        TValue Value { get; }
    }
}
