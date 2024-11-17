using DigitalSeal.Core.Exceptions;
using FluentAssertions;
using FluentAssertions.Primitives;
using LanguageExt.Common;
using LanguageExt.Pretty;
using Xunit.Sdk;

namespace DigitalSeal.Tests.Services
{
    public class TestExceptionHelper
    {
        public static XunitException ExpectedSuccess(Exception exception)
        {
            return new XunitException($"Expected success, but got failure: {exception.Message}");
        }

        public static XunitException ExpectedFailure()
        {
            return new XunitException("Expected failure result, but got success.");
        }

        public static AndConstraint<ObjectAssertions> AssertValidationEx(Exception exception)
        {
            return exception
                .Should().BeOfType<ValidationException>()
                .Should().NotBeNull();
        }

        //public static void AssertResultSuccess<TResult, U>(Result<TResult> result, Func<TResult, U> assert)
        //{
        //    result.IsSuccess.Should().BeTrue();
        //    result.IsFaulted.Should().BeFalse();
        //    _ = result.Match(
        //        assert,
        //        exception => throw ExpectedSuccess(exception));
        //}

        //public static void AssertResultFail<TResult, U>(Result<TResult> result, Func<Exception, U> assert)
        //{
        //    result.IsSuccess.Should().BeFalse();
        //    result.IsFaulted.Should().BeTrue();
        //    _ = result.Match(
        //        success => throw TestExceptionHelper.ExpectedFailure(),
        //        assert);
        //}
    }

    public static class ResultExtensions
    {
        public static void AssertSuccess<TResult, U>(this Result<TResult> result, Func<TResult, U> assert)
        {
            result.IsSuccess.Should().BeTrue();
            result.IsFaulted.Should().BeFalse();
            _ = result.Match(
                assert,
                exception => throw TestExceptionHelper.ExpectedSuccess(exception));
        }

        public static void AssertFailure<TResult, U>(this Result<TResult> result, Func<Exception, U> assert)
        {
            result.IsSuccess.Should().BeFalse();
            result.IsFaulted.Should().BeTrue();
            _ = result.Match(
                success => throw TestExceptionHelper.ExpectedFailure(),
                assert);
        }
    }
}
