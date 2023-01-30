using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RhoMicro.CodeAnalysis.Attributes
{
	internal abstract class AttributeFactory<T> : IAttributeFactory<T>
	{
		public static IAttributeFactory<T> Create()
		{
			var collection = new AttributeFactoryCollection<T>();

			Type type = typeof(T);
			PropertyInfo[] properties = type.GetProperties().Where(p => p.CanWrite).ToArray();
			ConstructorInfo[] constructors = type.GetConstructors().Where(c => !c.IsStatic).ToArray();

			for (Int32 ctorIndex = 0; ctorIndex < constructors.Length; ctorIndex++)
			{
				ConstructorInfo constructor = constructors[ctorIndex];

				var parameters = new ParameterExpression[2]
				{
							Expression.Parameter(typeof(AttributeSyntax), "attributeData"),
							Expression.Parameter(typeof(SemanticModel), "semanticModel"),
				};
				Func<AttributeSyntax, SemanticModel, Boolean> canBuildStrategy = getCanBuildStrategy();
				Func<AttributeSyntax, SemanticModel, T> buildStrategy = getBuildStrategy();
				IAttributeFactory<T> factory = Create(canBuildStrategy, buildStrategy);

				_ = collection.Add(factory);

				Func<AttributeSyntax, SemanticModel, T> getBuildStrategy()
				{
					Expression canBuildTest = getCanBuildTest();
					Expression ifTrue = getBuildExpr();
					Expression ifFalse = getThrowExpr($"Cannot build {typeof(T)} using the attribute syntax and semantic model provided.");
					ConditionalExpression body = Expression.Condition(canBuildTest, ifTrue, ifFalse);

					LambdaExpression lambda = Expression.Lambda(body, parameters);
					var strategy = (Func<AttributeSyntax, SemanticModel, T>)lambda.Compile();

					return strategy;

					Expression getBuildExpr()
					{
						ParameterInfo[] ctorParams = constructor.GetParameters().ToArray();

						ParameterExpression newInstanceExpr = Expression.Variable(type, "instance");

						var blockVariables = new List<ParameterExpression>();
						var blockExpressions = new List<Expression>();
						var typeParamSetExpressions = new List<Expression>();
						var typeParamVariables = new List<ParameterExpression>();

						Boolean implementsTypeParameterSetter = type.TryGetMethodSemantically(typeof(IHasTypeParameter).GetMethod(nameof(IHasTypeParameter.SetTypeParameter)), out MethodInfo typeParameterSetter);

						for (Int32 i = 0; i < ctorParams.Length; i++)
						{
							ParameterInfo parameter = ctorParams[i];

							Type outValueType = parameter.ParameterType;
							MethodInfo tryParseMethod = getTryParseMethod(outValueType);
							ParameterExpression outValue = Expression.Parameter(outValueType, $"argumentValue_{parameter.Name}");

							blockVariables.Add(outValue);

							Expression paramAssignmentExpr = null;

							if (!(parameter.ParameterType == typeof(Type) && implementsTypeParameterSetter))
							{
								MethodCallExpression callExpr = Expression.Call(null, tryParseMethod, parameters[0], parameters[1], outValue, Expression.Constant(i), Expression.Convert(Expression.Constant(null), typeof(String)), Expression.Constant(parameter.Name));

								Expression noArgReactionExpr = parameter.HasDefaultValue
									? Expression.Assign(outValue, Expression.Convert(Expression.Constant(parameter.DefaultValue), parameter.ParameterType))
									: getThrowExpr($"Missing argument for {parameter.Name} of type {parameter.ParameterType} encountered while attempting to construct instance of type {typeof(T)}.");
								paramAssignmentExpr = Expression.IfThen(Expression.Not(callExpr), noArgReactionExpr);
							}
							else
							{
								paramAssignmentExpr = Expression.Assign(outValue, Expression.Convert(Expression.Constant(null), outValueType));

								outValueType = typeof(Object);
								tryParseMethod = getTryParseMethod(outValueType);
								outValue = Expression.Parameter(outValueType, $"typeArgumentValue_{parameter.Name}");
								MethodCallExpression callExpr = Expression.Call(null, tryParseMethod, parameters[0], parameters[1], outValue, Expression.Constant(i), Expression.Convert(Expression.Constant(null), typeof(String)), Expression.Constant(parameter.Name));
								MethodCallExpression typeParamSetExpr = Expression.Call(newInstanceExpr, typeParameterSetter, Expression.Constant(parameter.Name), outValue);
								ConditionalExpression conditionalSetExpr = Expression.IfThen(callExpr, typeParamSetExpr);

								typeParamVariables.Add(outValue);
								typeParamSetExpressions.Add(conditionalSetExpr);
							}

							blockExpressions.Add(paramAssignmentExpr);
						}

						NewExpression newExpr = Expression.New(constructor, blockVariables);
						BinaryExpression newInstanceAssignmentExpr = Expression.Assign(newInstanceExpr, newExpr);

						blockVariables.AddRange(typeParamVariables);
						blockVariables.Add(newInstanceExpr);
						blockExpressions.Add(newInstanceAssignmentExpr);
						blockExpressions.AddRange(typeParamSetExpressions);

						Boolean implementsTypePropertySetter = type.TryGetMethodSemantically(typeof(IHasTypeProperty).GetMethod(nameof(IHasTypeProperty.SetTypeProperty)), out MethodInfo typePropertySetter);

						for (Int32 i = 0; i < properties.Length; i++)
						{
							PropertyInfo property = properties[i];

							Type outValueType = property.PropertyType;
							ParameterExpression outValue;
							Expression setConditionExpr;
							Expression setExpr;
							if (property.PropertyType == typeof(Type) && implementsTypePropertySetter)
							{
								setExpr = Expression.Call(newInstanceExpr, property.SetMethod, Expression.Convert(Expression.Constant(null), outValueType));

								outValueType = typeof(Object);
								MethodInfo tryParseMethod = getTryParseMethod(outValueType);
								outValue = Expression.Parameter(outValueType, $"typePropertyValue_{property.Name}");
								MethodCallExpression helperCallExpr = Expression.Call(null, tryParseMethod, parameters[0], parameters[1], outValue, Expression.Constant(-1), Expression.Constant(property.Name), Expression.Convert(Expression.Constant(null), typeof(String)));
								MethodCallExpression helperSetExpr = Expression.Call(newInstanceExpr, typePropertySetter, Expression.Constant(property.Name), outValue);

								BlockExpression conditionalBlock = Expression.Block(setExpr, helperSetExpr);

								setConditionExpr = Expression.IfThen(helperCallExpr, conditionalBlock);
							}
							else
							{
								MethodInfo tryParseMethod = getTryParseMethod(outValueType);
								outValue = Expression.Parameter(outValueType, $"propertyValue_{property.Name}");
								MethodCallExpression callExpr = Expression.Call(null, tryParseMethod, parameters[0], parameters[1], outValue, Expression.Constant(-1), Expression.Constant(property.Name), Expression.Convert(Expression.Constant(null), typeof(String)));
								setExpr = Expression.Call(newInstanceExpr, property.SetMethod, outValue);
								setConditionExpr = Expression.IfThen(callExpr, setExpr);
							}

							blockVariables.Add(outValue);
							blockExpressions.Add(setConditionExpr);
						}

						blockExpressions.Add(newInstanceExpr);

						BlockExpression block = Expression.Block(blockVariables, blockExpressions);

						return block;

						MethodInfo getTryParseMethod(Type forType)
						{
							String name = forType.IsArray ?
								nameof(Extensions.TryParseArrayArgument) :
								nameof(Extensions.TryParseArgument);
							Type constraint = forType.IsArray ? forType.GetElementType() : forType;

							MethodInfo method = typeof(Extensions).GetMethods()
								.Where(m => m.IsGenericMethod)
								.Select(m => m.MakeGenericMethod(constraint))
								.Single(m =>
								{
									ParameterInfo[] methodParams = m.GetParameters();
									return m.Name == name &&
										methodParams.Length == 6 &&
										methodParams[0].ParameterType == typeof(AttributeSyntax) &&
										methodParams[1].ParameterType == typeof(SemanticModel) &&
										methodParams[2].ParameterType == forType.MakeByRefType() &&
										methodParams[3].ParameterType == typeof(Int32) &&
										methodParams[4].ParameterType == typeof(String) &&
										methodParams[5].ParameterType == typeof(String);
								});

							return method;
						}
					}

					Expression getThrowExpr(String message)
					{
						ConstructorInfo ctorInfo = typeof(InvalidOperationException).GetConstructor(new[] { typeof(String) });
						ConstantExpression ctorParam = Expression.Constant(message);
						UnaryExpression throwExpr = Expression.Throw(Expression.New(ctorInfo, ctorParam));
						UnaryExpression returnExpr = Expression.Convert(Expression.Constant(null), type);
						BlockExpression throwBlock = Expression.Block(throwExpr, returnExpr);

						return throwBlock;
					}
				}

				Func<AttributeSyntax, SemanticModel, Boolean> getCanBuildStrategy()
				{
					Expression body = getCanBuildTest();

					LambdaExpression lambda = Expression.Lambda(body, parameters);
					var strategy = (Func<AttributeSyntax, SemanticModel, Boolean>)lambda.Compile();

					return strategy;
				}

				Expression getCanBuildTest()
				{
					ConstantExpression typeExpr = Expression.Constant(type, typeof(Type));
					MethodInfo getCtorsMethod = typeof(Type).GetMethod(nameof(Type.GetConstructors), new Type[] { });
					MethodCallExpression getCtorsCallExpr = Expression.Call(typeExpr, getCtorsMethod);

					MethodInfo whereMethod = typeof(Enumerable).GetMethods()
						.Where(m => m.Name == nameof(Enumerable.Where))
						.Select(m => m.MakeGenericMethod(typeof(ConstructorInfo)))
						.Single(m =>
						{
							ParameterInfo[] methodParameters = m.GetParameters();
							Boolean match = methodParameters.Length == 2 &&
										methodParameters[0].ParameterType == typeof(IEnumerable<ConstructorInfo>) &&
										methodParameters[1].ParameterType == typeof(Func<ConstructorInfo, Boolean>);

							return match;
						});
					ParameterExpression predicateParamExpr = Expression.Parameter(typeof(ConstructorInfo), "c");
					MethodInfo isStaticMethod = typeof(ConstructorInfo).GetProperty(nameof(ConstructorInfo.IsStatic)).GetMethod;
					LambdaExpression predicateExpr = Expression.Lambda(Expression.Not(Expression.Call(predicateParamExpr, isStaticMethod)), predicateParamExpr);
					MethodCallExpression whereCallExpr = Expression.Call(null, whereMethod, getCtorsCallExpr, predicateExpr);

					MethodInfo toArrayMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(ConstructorInfo));
					MethodCallExpression toArrayCall = Expression.Call(null, toArrayMethod, whereCallExpr);

					ConstantExpression ctorIndexExpr = Expression.Constant(ctorIndex);
					BinaryExpression indexAccessExpr = Expression.ArrayIndex(toArrayCall, ctorIndexExpr);

					MethodInfo matchesMethod = typeof(Extensions).GetMethods()
						.Single(m =>
						{
							ParameterInfo[] methodParams = m.GetParameters();
							return m.Name == nameof(Extensions.Matches) &&
								methodParams.Length == 3 &&
								methodParams[0].ParameterType == typeof(AttributeSyntax) &&
								methodParams[1].ParameterType == typeof(SemanticModel) &&
								methodParams[2].ParameterType == typeof(ConstructorInfo);
						});
					MethodCallExpression matchesCall = Expression.Call(null, matchesMethod, parameters[0], parameters[1], indexAccessExpr);

					return matchesCall;
				}
			}

			return collection;
		}
		public static IAttributeFactory<T> Create(Func<AttributeSyntax, SemanticModel, Boolean> canBuildStrategy, Func<AttributeSyntax, SemanticModel, T> buildStrategy)
		{
			return new AttributeFactoryStrategy<T>(canBuildStrategy, buildStrategy);
		}
		public static IAttributeFactory<T> Create(TypeIdentifier typeIdentifier, Func<AttributeSyntax, SemanticModel, T> buildStrategy)
		{
			return new AttributeFactoryStrategy<T>((d, s) => d.IsType(s, typeIdentifier), buildStrategy);
		}

		protected abstract T Build(AttributeSyntax attributeData, SemanticModel semanticModel);
		protected abstract Boolean CanBuild(AttributeSyntax attributeData, SemanticModel semanticModel);
		public Boolean TryBuild(AttributeSyntax attributeData, SemanticModel semanticModel, out T attribute)
		{
			if (CanBuild(attributeData, semanticModel))
			{
				attribute = Build(attributeData, semanticModel);
				return true;
			}

			attribute = default;
			return false;
		}
	}
}
