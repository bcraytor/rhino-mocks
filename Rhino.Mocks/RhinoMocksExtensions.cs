#region license
// Copyright (c) 2005 - 2007 Ayende Rahien (ayende@ayende.com)
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//     * Neither the name of Ayende Rahien nor the names of its
//     contributors may be used to endorse or promote products derived from this
//     software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

#if DOTNET35
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rhino.Mocks.Constraints;
using Rhino.Mocks.Exceptions;
using Rhino.Mocks.Generated;
using Rhino.Mocks.Interfaces;

namespace Rhino.Mocks
{
	/// <summary>
	/// A set of extension methods that adds Arrange Act Assert mode to Rhino Mocks
	/// </summary>
	public static class RhinoMocksExtensions
	{
		[ThreadStatic] internal static IList<Expression> argumentPredicates;

		/// <summary>
		/// Create an expectation on this mock for this action to occur
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="action">The action.</param>
		/// <returns></returns>
		public static IMethodOptions<VoidType> Expect<T>(this T mock, Action<T> action)
		{
			return Expect<T, VoidType>(mock, t =>
			{
				action(t);
				return null;
			});
		}

		/// <summary>
		/// Reset all expectations on this mock object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		public static void BackToRecord<T>(this T mock)
		{
			BackToRecord(mock, BackToRecordOptions.All);
		}

		/// <summary>
		/// Reset the selected expectation on this mock object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="options">The options to reset the expectations on this mock.</param>
		public static void BackToRecord<T>(this T mock, BackToRecordOptions options)
		{
			IMockedObject mockedObject = MockRepository.GetMockedObject(mock);
			mockedObject.Repository.BackToRecord(mock, options);
		}

		/// <summary>
		/// Gets the mock repository for this specificied mock object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <returns></returns>
		public static MockRepository GetMockRepository<T>(this T mock)
		{
			IMockedObject mockedObject = MockRepository.GetMockedObject(mock);
			return mockedObject.Repository;
		}

		/// <summary>
		/// Create an expectation on this mock for this action to occur
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="action">The action.</param>
		/// <returns></returns>
		public static IMethodOptions<R> Expect<T, R>(this T mock, Func<T, R> action)
		{
			IMockedObject mockedObject = MockRepository.GetMockedObject(mock);
			MockRepository mocks = mockedObject.Repository;
			mocks.BackToRecord(mock, BackToRecordOptions.None);
			action(mock);
			IMethodOptions<R> options = LastCall.GetOptions<R>();
			options.TentativeReturn();
			mocks.Replay(mock);
			return options;
		}

		/// <summary>
		/// Tell the mock object to perform a certain action when a matching 
		/// method is called.
		/// Does not create an expectation for this method.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="action">The action.</param>
		/// <returns></returns>
		public static IMethodOptions<object> Stub<T>(this T mock, Action<T> action)
		{
			return Stub<T, object>(mock, t =>
			{
				action(t);
				return null;
			});
		}

		/// <summary>
		/// Tell the mock object to perform a certain action when a matching
		/// method is called.
		/// Does not create an expectation for this method.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="R"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="action">The action.</param>
		/// <returns></returns>
		public static IMethodOptions<R> Stub<T, R>(this T mock, Func<T, R> action)
		{
			return Expect(mock, action).Repeat.Times(0, 1);
		}

		/// <summary>
		/// Gets the arguments for calls made on this mock object and the method that was called
		/// in the action.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="action">The action.</param>
		/// <returns></returns>
		/// <example>
		/// Here we will get all the arguments for all the calls made to DoSomething(int)
		/// <code>
		/// var argsForCalls = foo54.GetArgumentsForCallsMadeOn(x =&gt; x.DoSomething(0))
		/// </code>
		/// </example>
		public static IList<object[]> GetArgumentsForCallsMadeOn<T>(this T mock, Action<T> action)
		{
			return GetArgumentsForCallsMadeOn(mock, action, DefaultConstraintSetup);
		}

		/// <summary>
		/// Gets the arguments for calls made on this mock object and the method that was called
		/// in the action and matches the given constraints
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="action">The action.</param>
		/// <param name="setupConstraints">The setup constraints.</param>
		/// <returns></returns>
		/// <example>
		/// Here we will get all the arguments for all the calls made to DoSomething(int)
		/// <code>
		/// var argsForCalls = foo54.GetArgumentsForCallsMadeOn(x =&gt; x.DoSomething(0))
		/// </code>
		/// </example>
		public static IList<object[]> GetArgumentsForCallsMadeOn<T>(this T mock, Action<T> action, Action<IMethodOptions<object>> setupConstraints)
		{
			argumentPredicates = new List<Expression>();
			return GetExpectationsToVerify(mock, action, setupConstraints).ArgumentsForAllCalls;
		}

		/// <summary>
		/// Asserts that a particular method was called on this mock object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="action">The action.</param>
		public static void AssertWasCalled<T>(this T mock, Action<T> action)
		{
			argumentPredicates = new List<Expression>();
			AssertWasCalled(mock, action, DefaultConstraintSetup);
		}

		private static void DefaultConstraintSetup(IMethodOptions<object> options)
		{
			var constraints = new List<AbstractConstraint>();
			foreach (Expression expression in argumentPredicates)
			{
				constraints.Add(new LambdaConstraint(expression));
			}
			if (constraints.Count != 0)
				options.Constraints(constraints.ToArray());
		}

		/// <summary>
		/// Asserts that a particular method was called on this mock object that match
		/// a particular constraint set.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="action">The action.</param>
		/// <param name="setupConstraints">The setup constraints.</param>
		public static void AssertWasCalled<T>(this T mock, Action<T> action, Action<IMethodOptions<object>> setupConstraints)
		{
			try
			{
				ExpectationVerificationInformation verificationInformation = GetExpectationsToVerify(mock, action, setupConstraints);

				foreach (var args in verificationInformation.ArgumentsForAllCalls)
				{
					if (verificationInformation.Expected.IsExpected(args))
						return;
				}
				throw new ExpectationViolationException("Expected that " +
				                                        verificationInformation.ExpectationsToVerify[0].ErrorMessage +
				                                        " would be called, but it was not found on the actual calls made on the mocked object.");
			}
			finally
			{
				argumentPredicates = null;
			}
		}


		/// <summary>
		/// Asserts that a particular method was NOT called on this mock object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="action">The action.</param>
		public static void AssertWasNotCalled<T>(this T mock, Action<T> action)
		{
			argumentPredicates = new List<Expression>();
			AssertWasNotCalled(mock, action, delegate(IMethodOptions<object> options)
			{
				var constraints = new List<AbstractConstraint>();
				foreach (Expression expression in argumentPredicates)
				{
					constraints.Add(new LambdaConstraint(expression));
				}
				if (constraints.Count != 0)
					options.Constraints(constraints.ToArray());
			});
		}

		/// <summary>
		/// Asserts that a particular method was NOT called on this mock object that match
		/// a particular constraint set.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mock">The mock.</param>
		/// <param name="action">The action.</param>
		/// <param name="setupConstraints">The setup constraints.</param>
		public static void AssertWasNotCalled<T>(this T mock, Action<T> action,
		                                         Action<IMethodOptions<object>> setupConstraints)
		{
			try
			{
				ExpectationVerificationInformation verificationInformation = GetExpectationsToVerify(mock, action, setupConstraints);

				foreach (var args in verificationInformation.ArgumentsForAllCalls)
				{
					if (verificationInformation.Expected.IsExpected(args))
						throw new ExpectationViolationException("Expected that " +
						                                        verificationInformation.ExpectationsToVerify[0].ErrorMessage +
						                                        " would not be called, but it was found on the actual calls made on the mocked object.");
				}
			}
			finally
			{
				argumentPredicates = null;
			}
		}


		private static ExpectationVerificationInformation GetExpectationsToVerify<T>(T mock, Action<T> action,
		                                                                             Action<IMethodOptions<object>>
		                                                                             	setupConstraints)
		{
			IMockedObject mockedObject = MockRepository.GetMockedObject(mock);
			MockRepository mocks = mockedObject.Repository;

			if (mocks.IsInReplayMode(mockedObject) == false)
			{
				throw new InvalidOperationException(
					"Cannot assert on an object that is not in replay mode. Did you forget to call ReplayAll() ?");
			}

			var mockToRecordExpectation =
				(T) mocks.DynamicMock(mockedObject.ImplementedTypes[0], mockedObject.ConstructorArguments);
			action(mockToRecordExpectation);

			AssertExactlySingleExpectaton(mocks, mockToRecordExpectation);

			IMethodOptions<object> lastMethodCall = mocks.LastMethodCall<object>(mockToRecordExpectation);
			lastMethodCall.TentativeReturn();
			if (setupConstraints != null)
			{
				setupConstraints(lastMethodCall);
			}
			ExpectationsList expectationsToVerify = mocks.Replayer.GetAllExpectationsForProxy(mockToRecordExpectation);
			if (expectationsToVerify.Count == 0)
				throw new InvalidOperationException(
					"The expectation was removed from the waiting expectations list, did you call Repeat.Any() ? This is not supported in AssertWasCalled()");
			IExpectation expected = expectationsToVerify[0];
			ICollection<object[]> argumentsForAllCalls = mockedObject.GetCallArgumentsFor(expected.Method);
			return new ExpectationVerificationInformation
			       	{
						ArgumentsForAllCalls = new List<object[]>(argumentsForAllCalls),
			       		ExpectationsToVerify = expectationsToVerify,
			       		Expected = expected
			       	};
		}

		/// <summary>
		/// Verifies all expectations on this mock object
		/// </summary>
		/// <param name="mockObject">The mock object.</param>
		public static void VerifyAllExpectations(this object mockObject)
		{
			IMockedObject mockedObject = MockRepository.GetMockedObject(mockObject);
			mockedObject.Repository.Verify(mockedObject);
		}


		/// <summary>
		/// Gets the event raiser for the event that was called in the action passed
		/// </summary>
		/// <typeparam name="TEventSource">The type of the event source.</typeparam>
		/// <param name="mockObject">The mock object.</param>
		/// <param name="eventSubscription">The event subscription.</param>
		/// <returns></returns>
        public static IEventRaiser GetEventRaiser<TEventSource>(this TEventSource mockObject, Action<TEventSource> eventSubscription) 
        {
            return mockObject
                .Stub(eventSubscription)
                .IgnoreArguments()
                .GetEventRaiser();    
        }

		/// <summary>
		/// Raise the specified event using the passed arguments.
		/// The even is extracted from the passed labmda
		/// </summary>
		/// <typeparam name="TEventSource">The type of the event source.</typeparam>
		/// <param name="mockObject">The mock object.</param>
		/// <param name="eventSubscription">The event subscription.</param>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public static void Raise<TEventSource>(this TEventSource mockObject, Action<TEventSource> eventSubscription, object sender, EventArgs args) 
        {
            var eventRaiser = GetEventRaiser(mockObject, eventSubscription);
            eventRaiser.Raise(sender, args);
        }

		/// <summary>
		/// Raise the specified event using the passed arguments.
		/// The even is extracted from the passed labmda
		/// </summary>
		/// <typeparam name="TEventSource">The type of the event source.</typeparam>
		/// <param name="mockObject">The mock object.</param>
		/// <param name="eventSubscription">The event subscription.</param>
		/// <param name="args">The args.</param>
        public static void Raise<TEventSource>(this TEventSource mockObject, Action<TEventSource> eventSubscription, params object[] args) 
        {
            var eventRaiser = GetEventRaiser(mockObject, eventSubscription);
            eventRaiser.Raise(args);
        }

		private static void AssertExactlySingleExpectaton<T>(MockRepository mocks, T mockToRecordExpectation)
		{
			if (mocks.Replayer.GetAllExpectationsForProxy(mockToRecordExpectation).Count == 0)
				throw new InvalidOperationException(
					"No expectations were setup to be verified, ensure that the method call in the action is a virtual (C#) / overridable (VB.Net) method call");

			if (mocks.Replayer.GetAllExpectationsForProxy(mockToRecordExpectation).Count > 1)
				throw new InvalidOperationException(
					"You can only use a single expectation on AssertWasCalled(), use separate calls to AssertWasCalled() if you want to verify several expectations");
		}

		#region Nested type: VoidType

		/// <summary>
		/// Fake type that disallow creating it.
		/// Should have been System.Type, but we can't use it.
		/// </summary>
		public class VoidType
		{
			private VoidType()
			{
			}
		}

		#endregion
	}
}

#endif