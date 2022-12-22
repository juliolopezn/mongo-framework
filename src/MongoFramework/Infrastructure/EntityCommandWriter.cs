﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Diagnostics;
using MongoFramework.Infrastructure.Mapping;

namespace MongoFramework.Infrastructure
{
	public static class EntityCommandWriter
	{
		public static void Write<TEntity>(IMongoDbConnection connection, IEnumerable<IWriteCommand> commands, WriteModelOptions options) where TEntity : class
		{
			var writeModels = commands.OfType<IWriteCommand<TEntity>>().SelectMany(c => c.GetModel(options)).ToArray();
			if (writeModels.Any())
			{
				var entityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
				var collection = connection.GetDatabase().GetCollection<TEntity>(entityDefinition.CollectionName);
				using (var diagnostics = DiagnosticRunner.Start(connection, writeModels))
				{
					try
					{
						collection.BulkWrite(writeModels);
					}
					catch (Exception exception)
					{
						diagnostics.Error(exception);
						throw;
					}
				}
			}
		}
		public static async Task WriteAsync<TEntity>(IMongoDbConnection connection, IEnumerable<IWriteCommand> commands, WriteModelOptions options, CancellationToken cancellationToken = default) where TEntity : class
		{
			var writeModels = commands.OfType<IWriteCommand<TEntity>>().SelectMany(c => c.GetModel(options)).ToArray();
			if (writeModels.Any())
			{
				var entityDefinition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
				var collection = connection.GetDatabase().GetCollection<TEntity>(entityDefinition.CollectionName);
				using (var diagnostics = DiagnosticRunner.Start(connection, writeModels))
				{
					try
					{
						await collection.BulkWriteAsync(writeModels, cancellationToken: cancellationToken);
					}
					catch (Exception exception)
					{
						diagnostics.Error(exception);
						throw;
					}
				}
			}
		}
	}
}
