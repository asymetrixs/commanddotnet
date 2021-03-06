using System;
using System.Collections.Generic;
using System.Linq;
using CommandDotNet.Parsing;

namespace CommandDotNet.TypeDescriptors
{
    internal class ErrorReportingDescriptor: IArgumentTypeDescriptor, IAllowedValuesTypeDescriptor
    {
        private readonly IArgumentTypeDescriptor _innerDescriptor;

        public ErrorReportingDescriptor(IArgumentTypeDescriptor innerDescriptor)
        {
            _innerDescriptor = innerDescriptor;
        }

        public bool CanSupport(Type type)
        {
            return _innerDescriptor.CanSupport(type);
        }

        public string GetDisplayName(IArgument argument)
        {
            return _innerDescriptor.GetDisplayName(argument);
        }

        public object ParseString(IArgument argument, string value)
        {
            try
            {
                return _innerDescriptor.ParseString(argument, value);
            }
            catch (FormatException)
            {
                throw new ValueParsingException(
                    $"'{value}' is not a valid {_innerDescriptor.GetDisplayName(argument)}");
            }
            catch (ArgumentException)
            {
                throw new ValueParsingException(
                    $"'{value}' is not a valid {_innerDescriptor.GetDisplayName(argument)}");
            }
        }

        public IEnumerable<string> GetAllowedValues(IArgument argument)
        {
            return (_innerDescriptor as IAllowedValuesTypeDescriptor)?.GetAllowedValues(argument)
                   ?? Enumerable.Empty<string>();
        }
    }
}