using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RM.Lib.Utility
{
	public static class ThreadingExtensions
	{
		public static bool WaitedOrCancelled(this CancellationToken token, TimeSpan timeout)
		{
			return !token.IsCancellationRequested && !token.WaitHandle.WaitOne(timeout);
		}
		/*
		public static Task Then<T>(this T task, Action<T> successFunc = null, Action<Exception> failFunc = null) where T : Task
		{
			return task.ContinueWith(t =>
			{
				if (t.IsFaulted || t.IsCanceled)
				{
					var exception = UnwrapAggregate(t.Exception);
					if (failFunc != null)
					{
						failFunc(exception);
					}
					else
					{
						throw exception ?? new TaskCanceledException(t);
					}
				}

				successFunc?.Invoke((T)t);
			});
		}

		public static Task Then<T>(this T task, Func<T, Task> successFunc = null, Func<Exception, Task> failFunc = null) where T : Task
		{
			return task.ContinueWith(t =>
			{
				if (t.IsFaulted || t.IsCanceled)
				{
					var exception = UnwrapAggregate(t.Exception);
					if (failFunc != null)
					{
						return failFunc(exception);
					}
					else
					{
						throw exception ?? new TaskCanceledException(t);
					}
				}

				return successFunc?.Invoke((T)t);
			}).Unwrap();
		}

		public static Task<TRes> Then<T, TRes>(this T task, Func<T, TRes> successFunc = null, Func<Exception, TRes> failFunc = null) where T : Task
		{
			return task.ContinueWith(t =>
			{
				if (t.IsFaulted || t.IsCanceled)
				{
					var exception = UnwrapAggregate(t.Exception);
					return failFunc != null
								? Task.FromResult(failFunc(exception))
								: (exception != null ? Task.FromException<TRes>(exception) : Task.FromCanceled<TRes>(GetTaskCancellationToken(t)));
				}

				if (successFunc != null)
				{
					return Task.FromResult(successFunc((T)t));
				}

				if (t is Task<TRes> taskRes)
				{
					return taskRes;
				}

				throw new NotSupportedException($"Not supported default conversion from {typeof(T).FullName} to result type {typeof(TRes).FullName}");
			}).Unwrap();
		}

		public static Task<TRes> Then<T, TRes>(this T task, Func<T, Task<TRes>> successFunc = null, Func<Exception, Task<TRes>> failFunc = null) where T : Task
		{
			return task.ContinueWith(t =>
			{
				if (t.IsFaulted || t.IsCanceled)
				{
					var exception = UnwrapAggregate(t.Exception);
					return failFunc != null
							? failFunc(exception)
							: (exception != null ? Task.FromException<TRes>(exception) : Task.FromCanceled<TRes>(GetTaskCancellationToken(t)));
				}

				if (successFunc != null)
				{
					return successFunc((T)t);
				}

				if (t is Task<TRes> taskRes)
				{
					return taskRes;
				}

				throw new NotSupportedException($"Not supported default conversion from {typeof(T).FullName} to result type {typeof(TRes).FullName}");
			}).Unwrap();
		}
		*/
		public static Task Then(this Task task, Action successFunc = null, Action<Exception> failFunc = null)
		{
			return task.ContinueWith(t =>
					{
						if (t.IsFaulted || t.IsCanceled)
						{
							var exc = UnwrapAggregate(t.Exception);
							if (failFunc != null)
							{
								failFunc(exc);
							}
							else if(exc != null)
							{
								throw exc;
							}
						}
						else
						{
							successFunc?.Invoke();
						}
					});
		}

		public static Task<TRes> Then<T, TRes>(this Task<T> task, Func<T, Task<TRes>> successFunc = null, Func<Exception, Task<TRes>> failFunc = null)
		{
			return task.ContinueWith(t =>
					{
						if (t.IsFaulted || t.IsCanceled)
						{
							var exc = UnwrapAggregate(t.Exception);
							return failFunc != null
										? failFunc(exc)
										: exc != null ? Task.FromException<TRes>(exc) : Task.FromCanceled<TRes>(GetTaskCancellationToken(t));
						}

						return successFunc != null ? successFunc(t.Result) : Task.FromResult((TRes)Convert.ChangeType(task.Result, typeof(TRes)));
					}).Unwrap();
		}

		public static Task<TRes> Then<T, TRes>(this Task<T> task, Func<T, TRes> successFunc = null, Func<Exception, TRes> failFunc = null)
		{
			return task.ContinueWith(t =>
					{
						if (t.IsFaulted || t.IsCanceled)
						{
							var exc = UnwrapAggregate(t.Exception);
							return failFunc != null
										? failFunc(exc)
										: throw exc ?? new TaskCanceledException(t);
						}

						return successFunc != null ? successFunc(t.Result) : (TRes)Convert.ChangeType(task.Result, typeof(TRes));
					});
		}

		public static Task<(T1, T2)> WhenAll<T1, T2>(this (Task<T1>, Task<T2>) taskTuple)
		{
			return Task.WhenAll(
						taskTuple.Item1.Then(t1 => (object)t1),
						taskTuple.Item2.Then(t2 => (object)t2)
					).Then(objs => ((T1)objs[0], (T2)objs[1]));
		}

		private static Exception UnwrapAggregate(Exception ex)
		{
			return ex is AggregateException aggr ? aggr.Flatten().InnerException : ex;
		}

		private static CancellationToken GetTaskCancellationToken(Task task)
		{
			return (CancellationToken)typeof(Task).GetProperty("CancellationToken", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(task);
		}
	}
}
