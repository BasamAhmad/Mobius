﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Spark.CSharp.Interop;
using Microsoft.Spark.CSharp.Interop.Ipc;

namespace Microsoft.Spark.CSharp.Proxy.Ipc
{
    internal class DataFrameIpcProxy : IDataFrameProxy
    {
        private readonly JvmObjectReference jvmDataFrameReference;
        private readonly ISqlContextProxy sqlContextProxy;

        internal DataFrameIpcProxy(JvmObjectReference jvmDataFrameReference, ISqlContextProxy sqlProxy)
        {
            this.jvmDataFrameReference = jvmDataFrameReference;
            sqlContextProxy = sqlProxy;
        }

        public void RegisterTempTable(string tableName)
        {
            SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(jvmDataFrameReference,
                "registerTempTable", new object[] {tableName});
        }

        public long Count()
        {
            return
                long.Parse(
                    SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        jvmDataFrameReference, "count").ToString());
        }

        public string GetQueryExecution()
        {
            var queryExecutionReference = GetQueryExecutionReference();
            return SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(queryExecutionReference, "toString").ToString();
        }

        private JvmObjectReference GetQueryExecutionReference()
        {
            return
                new JvmObjectReference(
                    SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        jvmDataFrameReference, "queryExecution").ToString());
        }

        public string GetExecutedPlan()
        {
            var queryExecutionReference = GetQueryExecutionReference();
            var executedPlanReference =
                new JvmObjectReference(
                    SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(queryExecutionReference, "executedPlan")
                        .ToString());
            return SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(executedPlanReference, "toString", new object[] { }).ToString();
        }

        public string GetShowString(int numberOfRows, bool truncate)
        {
            return
                SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                    jvmDataFrameReference, "showString",
                    new object[] {numberOfRows /*,  truncate*/ }).ToString(); //1.4.1 does not support second param
        }

        public IStructTypeProxy GetSchema()
        {
            return
                new StructTypeIpcProxy(new JvmObjectReference(
                    SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        jvmDataFrameReference, "schema").ToString()));
        }

        public IRDDProxy ToJSON()
        {
            return new RDDIpcProxy(
                new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(jvmDataFrameReference, "toJSON")), 
                    "toJavaRDD")));
        }

        public IRDDProxy ToRDD()
        {
            return new RDDIpcProxy(
                new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallStaticJavaMethod("org.apache.spark.sql.api.csharp.SQLUtils", "dfToRowRDD", new object[] {jvmDataFrameReference})),
                    "toJavaRDD")));
        }

        public IColumnProxy GetColumn(string columnName)
        {
            return
                new ColumnIpcProxy(new JvmObjectReference(
                    SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        jvmDataFrameReference, "col", new object[] {columnName}).ToString()));
        }

        public IDataFrameProxy Select(string columnName, string[] columnNames)
        {
            return new DataFrameIpcProxy(new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                jvmDataFrameReference, 
                "select", 
                new object[] 
                { 
                    columnName,
                    new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallStaticJavaMethod("org.apache.spark.sql.api.csharp.SQLUtils", "toSeq", new object[] { columnNames })) 
                })), 
                sqlContextProxy);
        }

        public IDataFrameProxy SelectExpr(string[] columnExpressions)
        {
            return new DataFrameIpcProxy(new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                jvmDataFrameReference,
                "selectExpr",
                new object[] 
                { 
                    new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallStaticJavaMethod("org.apache.spark.sql.api.csharp.SQLUtils", "toSeq", new object[] { columnExpressions })) 
                })),
                sqlContextProxy);
        }

        public IDataFrameProxy Filter(string condition)
        {
            return
                new DataFrameIpcProxy(new JvmObjectReference(
                    SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        jvmDataFrameReference, "filter", new object[] { condition }).ToString()), sqlContextProxy);
        }


        public IGroupedDataProxy GroupBy(string firstColumnName, string[] otherColumnNames)
        {
            return
                new GroupedDataIpcProxy(new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        jvmDataFrameReference, 
                        "groupBy",
                        new object[] 
                        { 
                            firstColumnName, 
                            new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallStaticJavaMethod("org.apache.spark.sql.api.csharp.SQLUtils", "toSeq", new object[] { otherColumnNames })) 
                        })));
        }

        public IGroupedDataProxy GroupBy()
        {
            return
                new GroupedDataIpcProxy(new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        jvmDataFrameReference,
                        "groupBy",
                        new object[] 
                        { 
                            new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallStaticJavaMethod("org.apache.spark.sql.api.csharp.SQLUtils","toSeq", 
                                new object[] {new JvmObjectReference[0]}))
                        })));
        }

        public IDataFrameProxy Agg(IGroupedDataProxy scalaGroupedDataReference, Dictionary<string, string> columnNameAggFunctionDictionary)
        {
            var mapReference = new JvmObjectReference(SparkCLRIpcProxy.JvmBridge.CallConstructor("java.util.HashMap").ToString());
            foreach (var key in columnNameAggFunctionDictionary.Keys)
            {
                SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(mapReference, "put", new object[] { key, columnNameAggFunctionDictionary[key]});
            }
            return
                new DataFrameIpcProxy(new JvmObjectReference(
                    SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        (scalaGroupedDataReference as GroupedDataIpcProxy).ScalaGroupedDataReference, "agg", new object[] { mapReference }).ToString()), sqlContextProxy);
        }

        public IDataFrameProxy Join(IDataFrameProxy otherScalaDataFrameReference, string joinColumnName)
        {
            return
                new DataFrameIpcProxy(new JvmObjectReference(
                        SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(jvmDataFrameReference, "join", new object[]
                        {
                            (otherScalaDataFrameReference as DataFrameIpcProxy).jvmDataFrameReference,
                            joinColumnName
                        }).ToString()
                    ), sqlContextProxy);
        }

        public IDataFrameProxy Join(IDataFrameProxy otherScalaDataFrameReference, string[] joinColumnNames)
        {
            throw new NotSupportedException("Not supported in 1.4.1");

            //TODO - uncomment this in 1.5
            //var stringSequenceReference = new JvmObjectReference(
            //         SparkCLRIpcProxy.JvmBridge.CallStaticJavaMethod("org.apache.spark.sql.api.csharp.SQLUtils", "toSeq", new object[] { joinColumnNames }).ToString());

            //return
            //    new JvmObjectReference(
            //            SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(scalaDataFrameReference, "join", new object[]
            //            {
            //                otherScalaDataFrameReference,
            //                stringSequenceReference
            //            }).ToString()
            //        );
        }

        public IDataFrameProxy Join(IDataFrameProxy otherScalaDataFrameReference, IColumnProxy scalaColumnReference, string joinType)
        {
            return
                new DataFrameIpcProxy(new JvmObjectReference(
                    SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        jvmDataFrameReference, "join",
                        new object[]
                        {
                            (otherScalaDataFrameReference as DataFrameIpcProxy).jvmDataFrameReference,
                            (scalaColumnReference as ColumnIpcProxy).ScalaColumnReference,
                            joinType
                        }).ToString()), sqlContextProxy);
        }

        public IDataFrameProxy Intersect(IDataFrameProxy otherScalaDataFrameReference) 
        { 
            return  
                new DataFrameIpcProxy(new JvmObjectReference( 
                    SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod( 
                        jvmDataFrameReference, "intersect",  
                        new object[]{(otherScalaDataFrameReference as DataFrameIpcProxy).jvmDataFrameReference}).ToString()), sqlContextProxy); 
        } 
    }

    internal class UDFIpcProxy : IUDFProxy
    {
        private readonly JvmObjectReference jvmUDFReference;

        internal UDFIpcProxy(JvmObjectReference jvmUDFReference)
        {
            this.jvmUDFReference = jvmUDFReference;
        }
        
        public IColumnProxy Apply(IColumnProxy[] columns)
        {
            var seq = new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallStaticJavaMethod("org.apache.spark.sql.api.csharp.SQLUtils",
                        "toSeq", new object[] { columns.Select(c => (c as ColumnIpcProxy).ScalaColumnReference).ToArray() }));
            return new ColumnIpcProxy(new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(jvmUDFReference, "apply", seq)));
        }
    }

    internal class ColumnIpcProxy : IColumnProxy
    {
        private readonly JvmObjectReference scalaColumnReference;

        internal JvmObjectReference ScalaColumnReference { get { return scalaColumnReference; } }

        internal ColumnIpcProxy(JvmObjectReference colReference)
        {
            scalaColumnReference = colReference;
        }

        public IColumnProxy EqualsOperator(IColumnProxy secondColumn)
        {
            return
                new ColumnIpcProxy(new JvmObjectReference(
                    SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(
                        scalaColumnReference, "equalTo",
                        new object[] { (secondColumn as ColumnIpcProxy).scalaColumnReference }).ToString()));
        }

        public IColumnProxy UnaryOp(string name)
        {
            return new ColumnIpcProxy(new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(scalaColumnReference, name)));
        }

        public IColumnProxy FuncOp(string name)
        {
            return new ColumnIpcProxy(new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallStaticJavaMethod("org.apache.spark.sql.functions", name, scalaColumnReference)));
        }

        public IColumnProxy BinOp(string name, object other)
        {
            if (other is ColumnIpcProxy)
                other = (other as ColumnIpcProxy).scalaColumnReference;
            return new ColumnIpcProxy(new JvmObjectReference((string)SparkCLRIpcProxy.JvmBridge.CallNonStaticJavaMethod(scalaColumnReference, name, other)));
        }
    }

    internal class GroupedDataIpcProxy : IGroupedDataProxy
    {
        private readonly JvmObjectReference scalaGroupedDataReference;
        internal JvmObjectReference ScalaGroupedDataReference { get { return scalaGroupedDataReference; } }

        internal GroupedDataIpcProxy(JvmObjectReference gdRef)
        {
            scalaGroupedDataReference = gdRef;
        }
    }

}
