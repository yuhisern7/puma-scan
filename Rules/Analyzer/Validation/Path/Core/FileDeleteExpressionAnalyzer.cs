/* 
 * Copyright(c) 2016 - 2017 Puma Security, LLC (https://www.pumascan.com)
 * 
 * Project Leader: Eric Johnson (eric.johnson@pumascan.com)
 * Lead Developer: Eric Mead (eric.mead@pumascan.com)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 */

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Puma.Security.Rules.Analyzer.Core;
using Puma.Security.Rules.Common.Extensions;

namespace Puma.Security.Rules.Analyzer.Validation.Path.Core
{
    public class FileDeleteExpressionAnalyzer : IFileDeleteExpressionAnalyzer
    {
        public bool IsVulnerable(SemanticModel model, InvocationExpressionSyntax syntax)
        {
            if (!ContainsFileDeleteCommands(syntax)) return false;

            var symbol = model.GetSymbolInfo(syntax).Symbol as IMethodSymbol;

            if (!IsFileDeleteCommand(symbol)) return false;

            if (syntax.ArgumentList.Arguments.Count > 0)
            {
                var argSyntax = syntax.ArgumentList.Arguments[0].Expression;
                var expressionAnalyzer = ExpressionSyntaxAnalyzerFactory.Create(argSyntax);
                if (expressionAnalyzer.CanSuppress(model, argSyntax))
                {
                    return false;
                }

                //TODO: if still vulnerable after eliminating any low hanging fruit - then we need to perform data flow analysis
            }

            return true;
        }

        private static bool ContainsFileDeleteCommands(InvocationExpressionSyntax syntax)
            => syntax.ToString().Contains("File.Delete");

        private bool IsFileDeleteCommand(IMethodSymbol symbol) => symbol.IsMethod("System.IO.File", "Delete");
    }
}