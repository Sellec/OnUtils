using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.Mvc.ExpressionUtil
{
	internal static class CachedExpressionCompiler
	{
		private static class Compiler<TIn, TOut>
		{
			private static Func<TIn, TOut> _identityFunc;

			private static readonly ConcurrentDictionary<MemberInfo, Func<TIn, TOut>> _simpleMemberAccessDict = new ConcurrentDictionary<MemberInfo, Func<TIn, TOut>>();

			private static readonly ConcurrentDictionary<MemberInfo, Func<object, TOut>> _constMemberAccessDict = new ConcurrentDictionary<MemberInfo, Func<object, TOut>>();

			private static readonly ConcurrentDictionary<ExpressionFingerprintChain, Hoisted<TIn, TOut>> _fingerprintedCache = new ConcurrentDictionary<ExpressionFingerprintChain, Hoisted<TIn, TOut>>();

			public static Func<TIn, TOut> Compile(Expression<Func<TIn, TOut>> expr)
			{
				Func<TIn, TOut> arg_2E_0;
				if ((arg_2E_0 = CachedExpressionCompiler.Compiler<TIn, TOut>.CompileFromIdentityFunc(expr)) == null && (arg_2E_0 = CachedExpressionCompiler.Compiler<TIn, TOut>.CompileFromConstLookup(expr)) == null && (arg_2E_0 = CachedExpressionCompiler.Compiler<TIn, TOut>.CompileFromMemberAccess(expr)) == null)
				{
					arg_2E_0 = (CachedExpressionCompiler.Compiler<TIn, TOut>.CompileFromFingerprint(expr) ?? CachedExpressionCompiler.Compiler<TIn, TOut>.CompileSlow(expr));
				}
				return arg_2E_0;
			}

			private static Func<TIn, TOut> CompileFromConstLookup(Expression<Func<TIn, TOut>> expr)
			{
				ConstantExpression constantExpression = expr.Body as ConstantExpression;
				if (constantExpression != null)
				{
					TOut constantValue = (TOut)((object)constantExpression.Value);
					return (TIn _) => constantValue;
				}
				return null;
			}

			private static Func<TIn, TOut> CompileFromIdentityFunc(Expression<Func<TIn, TOut>> expr)
			{
				if (expr.Body == expr.Parameters[0])
				{
					if (CachedExpressionCompiler.Compiler<TIn, TOut>._identityFunc == null)
					{
						CachedExpressionCompiler.Compiler<TIn, TOut>._identityFunc = expr.Compile();
					}
					return CachedExpressionCompiler.Compiler<TIn, TOut>._identityFunc;
				}
				return null;
			}

			private static Func<TIn, TOut> CompileFromFingerprint(Expression<Func<TIn, TOut>> expr)
			{
				List<object> capturedConstants;
				ExpressionFingerprintChain fingerprintChain = FingerprintingExpressionVisitor.GetFingerprintChain(expr, out capturedConstants);
				if (fingerprintChain != null)
				{
					Hoisted<TIn, TOut> del = CachedExpressionCompiler.Compiler<TIn, TOut>._fingerprintedCache.GetOrAdd(fingerprintChain, delegate(ExpressionFingerprintChain _)
					{
						Expression<Hoisted<TIn, TOut>> expression = HoistingExpressionVisitor<TIn, TOut>.Hoist(expr);
						return expression.Compile();
					});
					return (TIn model) => del(model, capturedConstants);
				}
				return null;
			}

			private static Func<TIn, TOut> CompileFromMemberAccess(Expression<Func<TIn, TOut>> expr)
			{
				MemberExpression memberExpr = expr.Body as MemberExpression;
				if (memberExpr != null)
				{
					if (memberExpr.Expression == expr.Parameters[0] || memberExpr.Expression == null)
					{
						return CachedExpressionCompiler.Compiler<TIn, TOut>._simpleMemberAccessDict.GetOrAdd(memberExpr.Member, (MemberInfo _) => expr.Compile());
					}
					ConstantExpression constantExpression = memberExpr.Expression as ConstantExpression;
					if (constantExpression != null)
					{
						Func<object, TOut> del = CachedExpressionCompiler.Compiler<TIn, TOut>._constMemberAccessDict.GetOrAdd(memberExpr.Member, delegate(MemberInfo _)
						{
							ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "capturedLocal");
							UnaryExpression expression = Expression.Convert(parameterExpression, memberExpr.Member.DeclaringType);
							MemberExpression body = memberExpr.Update(expression);
							Expression<Func<object, TOut>> expression2 = Expression.Lambda<Func<object, TOut>>(body, new ParameterExpression[]
							{
								parameterExpression
							});
							return expression2.Compile();
						});
						object capturedLocal = constantExpression.Value;
						return (TIn _) => del(capturedLocal);
					}
				}
				return null;
			}

			private static Func<TIn, TOut> CompileSlow(Expression<Func<TIn, TOut>> expr)
			{
				return expr.Compile();
			}
		}

		public static Func<TModel, TValue> Process<TModel, TValue>(Expression<Func<TModel, TValue>> lambdaExpression)
		{
			return CachedExpressionCompiler.Compiler<TModel, TValue>.Compile(lambdaExpression);
		}
	}
}
