using System;

// ReSharper disable once CheckNamespace
namespace log4net
{
	public interface ILog
	{
		void Error(Exception ex);
	}

	public class LoggeerMock : ILog
	{
		public void Error(Exception ex)
		{
			//
		}
	}

	public static class LogManager
	{
		public static ILog GetLogger(Type type)
		{
			return new LoggeerMock();
		}
	}
}
