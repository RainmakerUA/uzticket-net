using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RM.Lib.Utility
{
	public static class ExpressionExtensions
	{
		private static readonly Dictionary<int, Delegate> _delegateCache = new Dictionary<int, Delegate>();

		public static Action<T1, T2> SetReceiver<TRec, T1, T2>(this Expression<Action<TRec, T1, T2>> expr, TRec receiver)
		{
			var cacheKey = CombineHashes(expr.GetHashCode(), receiver.GetHashCode());

			if (_delegateCache.TryGetValue(cacheKey, out var deleg) && deleg is Action<T1, T2> result)
			{
				return result;
			}

			if (expr.Body is MethodCallExpression callExpr && callExpr.Object?.Type == typeof(TRec))
			{
				result = CreateDelegate<Action<T1, T2>, TRec>(callExpr, expr.Parameters, 2, receiver);
			}
			else
			{
				var lambda = expr.Compile();
				result = (arg1, arg2) => lambda(receiver, arg1, arg2);
			}

			return SetCache(cacheKey, result);
		}

		public static Func<T1, T2, T3, TRes> SetReceiver<TRec, T1, T2, T3, TRes>(this Expression<Func<TRec, T1, T2, T3, TRes>> expr, TRec receiver)
		{
			var cacheKey = CombineHashes(expr.GetHashCode(), receiver.GetHashCode());

			if (_delegateCache.TryGetValue(cacheKey, out var deleg) && deleg is Func<T1, T2, T3, TRes> result)
			{
				return result;
			}

			if (expr.Body is MethodCallExpression callExpr && callExpr.Object?.Type == typeof(TRec))
			{
				result = CreateDelegate<Func<T1, T2, T3, TRes>, TRec>(callExpr, expr.Parameters, 3, receiver);
			}
			else
			{
				var lambda = expr.Compile();
				result = (arg1, arg2, arg3) => lambda(receiver, arg1, arg2, arg3);
			}

			return SetCache(cacheKey, result);
		}

		public static Expression<Action<TRec, T1, T2>> MakeActionExpression<TRec, T1, T2>(this string methodName)
		{
			return MakeLambdaExpr<Action<TRec, T1, T2>>(typeof(TRec), methodName, typeof(T1), typeof(T2));
		}

		public static Expression<Func<TRec, T1, T2, T3, TRes>> MakeFuncExpression<TRec, T1, T2, T3, TRes>(this string methodName)
		{
			return MakeLambdaExpr<Func<TRec, T1, T2, T3, TRes>>(typeof(TRec), methodName, typeof(T1), typeof(T2), typeof(T3));
		}

		private static TDel CreateDelegate<TDel, TRec>(MethodCallExpression methodExpr, IEnumerable<ParameterExpression> exprParams, int outerArgsCount, TRec receiver) where TDel : Delegate
		{
			var args = methodExpr.Arguments;

			if (methodExpr.Arguments.Count == outerArgsCount) //TODO: Check types?
			{
				return (TDel)methodExpr.Method.CreateDelegate(typeof(TDel), receiver);
			}

			var recCallExpr = methodExpr.Update(Expression.Constant(receiver), args);
			var newExpr = Expression.Lambda<TDel>(recCallExpr, exprParams.Skip(1));
			return newExpr.Compile();
		}

		private static Expression<TDel> MakeLambdaExpr<TDel>(Type recvType, string methodName, params Type[] argTypes)
		{
			var method = recvType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			if (method == null)
			{
				throw new ArgumentException($"Method {recvType.FullName}.{methodName}({String.Join(", ", Array.ConvertAll(argTypes, t => t.Name))}) was not found!");
			}

			var receiver = Expression.Parameter(recvType, "receiver");
			var paramExprs = Array.ConvertAll(argTypes, Expression.Parameter);
			var call = Expression.Call(receiver, method, paramExprs);
			return Expression.Lambda<TDel>(call, $"{recvType.Name}_{methodName}", Prepend(paramExprs, receiver));
		}

		private static int CombineHashes(int hash1, int hash2)
		{
			return (((hash1 << 5) | (hash1 >> 27)) + hash1 ) ^ hash2;
		}

		private static TDel SetCache<TDel>(int key, TDel deleg) where TDel : Delegate
		{
			_delegateCache.Add(key, deleg);
			return deleg;
		}

		private static IEnumerable<T> Prepend<T>(IEnumerable<T> sequence, T item)
		{
			yield return item;

			foreach (var seqItem in sequence)
			{
				yield return seqItem;
			}
		}
	}
}
