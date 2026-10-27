using RoomBooking.Api.Dtos;

namespace RoomBooking.Api.Services;

public enum FailureKind
{
    None,
    NotFound,
    Conflict,
    Invalid
}

public class ServiceResult<T>
{
    public T? Value { get; init; }
    public ApiError? Error { get; init; }
    public FailureKind Failure { get; init; }

    public bool IsSuccess => Failure == FailureKind.None;

    public static ServiceResult<T> Ok(T value) =>
        new()
        {
            Value = value,
            Failure = FailureKind.None
        };

    public static ServiceResult<T> Fail(
        FailureKind kind,
        ApiError error) =>
        new()
        {
            Error = error,
            Failure = kind
        };
}