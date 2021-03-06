﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentAutomation.Exceptions;
using FluentAutomation.Interfaces;

namespace FluentAutomation
{
    public class ExpectProvider : IExpectProvider
    {
        private readonly ICommandProvider commandProvider = null;

        public ExpectProvider(ICommandProvider commandProvider)
        {
            this.commandProvider = commandProvider;
        }

        #region Count
        public void Count(string selector, int count)
        {
            this.commandProvider.Act(() =>
            {
                var unwrappedElements = this.commandProvider.FindMultiple(selector)() as IEnumerable<IElement>;
                if (unwrappedElements.Count() != count)
                {
                    throw new FluentExpectFailedException("Expected count of elements matching selector [{0}] to be [{1}] but instead it was [{2}]", selector, count, unwrappedElements.Count());
                }
            });
        }

        public void Count(Func<IEnumerable<IElement>> elements, int count)
        {
            this.commandProvider.Act(() =>
            {
                var unwrappedElements = elements() as IEnumerable<IElement>;
                if (unwrappedElements.Count() != count)
                {
                    throw new FluentExpectFailedException("Expected count of elements in collection to be [{1}] but instead it was [{2}]", count, unwrappedElements.Count());
                }
            });
        }
        #endregion

        #region CSS Class
        public void CssClass(string selector, string className)
        {
            this.commandProvider.Act(() =>
            {
                var unwrappedElement = this.commandProvider.Find(selector)();
                var elementClassAttributeValue = unwrappedElement.Attributes.Get("class").Trim();
                if (!HasCssClass(className, elementClassAttributeValue))
                {
                    throw new FluentExpectFailedException("Expected element [{0}] to include CSS class [{1}] but current class attribute is [{2}].", selector, className, elementClassAttributeValue);
                }
            });
        }

        public void CssClass(Func<IElement> element, string className)
        {
            this.commandProvider.Act(() =>
            {
                var unwrappedElement = element();
                var elementClassAttributeValue = unwrappedElement.Attributes.Get("class").Trim();
                if (!HasCssClass(className, elementClassAttributeValue))
                {
                    throw new FluentExpectFailedException("Expected element to include CSS class [{0}] but current class attribute is [{1}].", className, elementClassAttributeValue);
                }
            });
        }

        private bool HasCssClass(string className, string elementClassAttributeValue)
        {
            className = className.Replace(".", "").Trim();
            var hasClass = false;

            if (elementClassAttributeValue.Contains(' '))
            {
                elementClassAttributeValue.Split(' ').ToList().ForEach(cssClass =>
                {
                    cssClass = cssClass.Trim();
                    if (!string.IsNullOrEmpty(cssClass) && className.Equals(cssClass))
                    {
                        hasClass = true;
                        return;
                    }
                });
            }
            else
            {
                if (className.Equals(elementClassAttributeValue))
                {
                    hasClass = true;
                }
            }

            return hasClass;
        }
        #endregion

        #region Text
        public void Text(string selector, string text)
        {
            this.commandProvider.Act(() =>
            {
                var unwrappedElement = this.commandProvider.Find(selector)();
                if (unwrappedElement.IsText)
                {
                    if (!IsTextMatch(unwrappedElement.Text, text))
                    {
                        throw new FluentExpectFailedException("Expected TextElement [{0}] text to be [{1}] but it was actually [{2}].", selector, text, unwrappedElement.Text);
                    }
                }
                else if (unwrappedElement.IsSelect)
                {
                    if (unwrappedElement.IsMultipleSelect)
                    {
                        var foundMatch = false;
                        foreach (var optionText in unwrappedElement.SelectedOptionTextCollection)
                        {
                            if (IsTextMatch(optionText, text))
                            {
                                foundMatch = true;
                                break;
                            }
                        }

                        if (!foundMatch)
                        {
                            throw new FluentExpectFailedException("Expected SelectElement [{0}] selected options to have at least one option with text of [{1}]. Selected option text values include [{2}]", selector, text, string.Join(",", unwrappedElement.SelectedOptionTextCollection));
                        }
                    }
                    else
                    {
                        if (!IsTextMatch(unwrappedElement.Text, text))
                        {
                            throw new FluentExpectFailedException("Expected SelectElement [{0}] selected option text to be [{1}] but it was actually [{2}].", selector, text, unwrappedElement.Text);
                        }
                    }
                }
                else
                {
                    if (!IsTextMatch(unwrappedElement.Text, text))
                    {
                        throw new FluentExpectFailedException("Expected DOM Element [{0}] text to be [{1}] but it was actually [{2}].", selector, text, unwrappedElement.Text);
                    }
                }
            });
        }

        public void Text(Func<IElement> element, string text)
        {
            this.commandProvider.Act(() =>
            {
                var unwrappedElement = element();
                if (unwrappedElement.IsText)
                {
                    if (!IsTextMatch(unwrappedElement.Text, text))
                    {
                        throw new FluentExpectFailedException("Expected TextElement text to be [{1}] but it was actually [{2}].", text, unwrappedElement.Text);
                    }
                }
                else if (unwrappedElement.IsSelect)
                {
                    if (unwrappedElement.IsMultipleSelect)
                    {
                        var foundMatch = false;
                        foreach (var optionText in unwrappedElement.SelectedOptionTextCollection)
                        {
                            if (IsTextMatch(optionText, text))
                            {
                                foundMatch = true;
                                break;
                            }
                        }

                        if (!foundMatch)
                        {
                            throw new FluentExpectFailedException("Expected SelectElement selected options to have at least one option with text of [{0}]. Selected option text values include [{1}]", text, string.Join(",", unwrappedElement.SelectedOptionTextCollection));
                        }
                    }
                    else
                    {
                        if (!IsTextMatch(unwrappedElement.Text, text))
                        {
                            throw new FluentExpectFailedException("Expected SelectElement selected option text to be [{1}] but it was actually [{2}].", text, unwrappedElement.Text);
                        }
                    }
                }
                else
                {
                    if (!IsTextMatch(unwrappedElement.Text, text))
                    {
                        throw new FluentExpectFailedException("Expected DOM Element text to be [{1}] but it was actually [{2}].", text, unwrappedElement.Text);
                    }
                }
            });
        }

        public void Text(string selector, Expression<Func<string, bool>> matchFunc)
        {
            this.commandProvider.Act(() =>
            {
                var compiledFunc = matchFunc.Compile();
                var unwrappedElement = this.commandProvider.Find(selector)();
                if (unwrappedElement.IsText)
                {
                    if (!compiledFunc(unwrappedElement.Text))
                    {
                        throw new FluentExpectFailedException("Expected TextElement [{0}] text to match expression [{1}] but it was actually [{2}].", selector, matchFunc.ToExpressionString(), unwrappedElement.Text);
                    }
                }
                else if (unwrappedElement.IsSelect)
                {
                    if (unwrappedElement.IsMultipleSelect)
                    {
                        var foundMatch = false;
                        foreach (var optionText in unwrappedElement.SelectedOptionTextCollection)
                        {
                            if (compiledFunc(optionText))
                            {
                                foundMatch = true;
                                break;
                            }
                        }

                        if (!foundMatch)
                        {
                            throw new FluentExpectFailedException("Expected SelectElement [{0}] selected options to have at least one option with text matching expression[{1}]. Selected option text values include [{2}]", selector, matchFunc.ToExpressionString(), string.Join(",", unwrappedElement.SelectedOptionTextCollection));
                        }
                    }
                    else
                    {
                        if (!compiledFunc(unwrappedElement.Text))
                        {
                            throw new FluentExpectFailedException("Expected SelectElement [{0}] selected option text to match expression [{1}] but it was actually [{2}].", selector, matchFunc.ToExpressionString(), unwrappedElement.Text);
                        }
                    }
                }
                else
                {
                    if (!compiledFunc(unwrappedElement.Text))
                    {
                        throw new FluentExpectFailedException("Expected DOM Element [{0}] text to match expression [{1}] but it was actually [{2}].", selector, matchFunc.ToExpressionString(), unwrappedElement.Text);
                    }
                }
            });
        }

        public void Text(Func<IElement> element, Expression<Func<string, bool>> matchFunc)
        {
            this.commandProvider.Act(() =>
            {
                var compiledFunc = matchFunc.Compile();
                var unwrappedElement = element();
                if (unwrappedElement.IsText)
                {
                    if (!compiledFunc(unwrappedElement.Text))
                    {
                        throw new FluentExpectFailedException("Expected TextElement text to match expression [{0}] but it was actually [{1}].", matchFunc.ToExpressionString(), unwrappedElement.Text);
                    }
                }
                else if (unwrappedElement.IsSelect)
                {
                    if (unwrappedElement.IsMultipleSelect)
                    {
                        var foundMatch = false;
                        foreach (var optionText in unwrappedElement.SelectedOptionTextCollection)
                        {
                            if (compiledFunc(optionText))
                            {
                                foundMatch = true;
                                break;
                            }
                        }

                        if (!foundMatch)
                        {
                            throw new FluentExpectFailedException("Expected SelectElement selected options to have at least one option with text matching expression [{0}]. Selected option text values include [{1}]", matchFunc.ToExpressionString(), string.Join(",", unwrappedElement.SelectedOptionTextCollection));
                        }
                    }
                    else
                    {
                        if (!compiledFunc(unwrappedElement.Text))
                        {
                            throw new FluentExpectFailedException("Expected SelectElement selected option text to match expression [{0}] but it was actually [{1}].", matchFunc.ToExpressionString(), unwrappedElement.Text);
                        }
                    }
                }
                else
                {
                    if (!compiledFunc(unwrappedElement.Text))
                    {
                        throw new FluentExpectFailedException("Expected DOM Element text to match expression [{0}] but it was actually [{1}].", matchFunc.ToExpressionString(), unwrappedElement.Text);
                    }
                }
            });
        }

        private bool IsTextMatch(string elementText, string text)
        {
            return elementText.Equals(text, StringComparison.InvariantCultureIgnoreCase);
        }
        #endregion

        #region Value
        public void Value(string selector, string value)
        {
            this.commandProvider.Act(() =>
            {
                var unwrappedElement = this.commandProvider.Find(selector)();
                if (unwrappedElement.IsText)
                {
                    if (!IsTextMatch(unwrappedElement.Value, value))
                    {
                        throw new FluentExpectFailedException("Expected TextElement [{0}] selected option value to be [{1}] but it was actually [{2}].", selector, value, unwrappedElement.Text);
                    }
                }
                else if (unwrappedElement.IsSelect)
                {
                    var foundMatch = false;
                    foreach (var optionValue in unwrappedElement.SelectedOptionValues)
                    {
                        if (IsTextMatch(optionValue, value))
                        {
                            foundMatch = true;
                            break;
                        }
                    }

                    if (!foundMatch)
                    {
                        if (unwrappedElement.IsMultipleSelect)
                        {
                            throw new FluentExpectFailedException("Expected SelectElement [{0}] selected options to have at least one option with value of [{1}]. Selected option text values include [{2}]", selector, value, unwrappedElement.Value);
                        }
                        else
                        {
                            throw new FluentExpectFailedException("Expected SelectElement [{0}] selected option value to be [{1}] but it was actually [{2}].", selector, value, unwrappedElement.Value);
                        }
                    }
                }
                else
                {
                    if (!IsTextMatch(unwrappedElement.Value, value))
                    {
                        throw new FluentExpectFailedException("Expected element [{0}] value to be [{1}] but it was actually [{2}].", selector, value, unwrappedElement.Value);
                    }
                }
            });
        }

        public void Value(string selector, Expression<Func<string, bool>> matchFunc)
        {
            this.commandProvider.Act(() =>
            {
                var compiledFunc = matchFunc.Compile();
                var unwrappedElement = this.commandProvider.Find(selector)();
                if (unwrappedElement.IsText)
                {
                    if (!compiledFunc(unwrappedElement.Value))
                    {
                        throw new FluentExpectFailedException("Expected TextElement [{0}] value to match expression [{1}] but it was actually [{2}].", selector, matchFunc.ToExpressionString(), unwrappedElement.Value);
                    }
                }
                else if (unwrappedElement.IsSelect)
                {
                    if (unwrappedElement.IsMultipleSelect)
                    {
                        var foundMatch = false;
                        foreach (var optionValue in unwrappedElement.SelectedOptionValues)
                        {
                            if (compiledFunc(optionValue))
                            {
                                foundMatch = true;
                                break;
                            }
                        }

                        if (!foundMatch)
                        {
                            throw new FluentExpectFailedException("Expected SelectElement [{0}] selected options to have at least one option with value matching expression [{1}]. Selected option values include [{2}]", selector, matchFunc.ToExpressionString(), string.Join(",", unwrappedElement.SelectedOptionValues));
                        }
                    }
                    else
                    {
                        if (!compiledFunc(unwrappedElement.Text))
                        {
                            throw new FluentExpectFailedException("Expected SelectElement [{0}] selected option value to match expression [{1}] but it was actually [{2}].", selector, matchFunc.ToExpressionString(), unwrappedElement.Value);
                        }
                    }
                }
                else
                {
                    if (!compiledFunc(unwrappedElement.Value))
                    {
                        throw new FluentExpectFailedException("Expected element [{0}] value to match expression [{1}] but it was actually [{2}].", selector, matchFunc.ToExpressionString(), unwrappedElement.Value);
                    }
                }
            });
        }

        public void Value(Func<IElement> element, string value)
        {
            this.commandProvider.Act(() =>
            {
                var unwrappedElement = element();
                if (unwrappedElement.IsText)
                {
                    if (!IsTextMatch(unwrappedElement.Value, value))
                    {
                        throw new FluentExpectFailedException("Expected TextElement selected option value to be [{0}] but it was actually [{1}].", value, unwrappedElement.Text);
                    }
                }
                else if (unwrappedElement.IsSelect)
                {
                    if (unwrappedElement.IsMultipleSelect)
                    {
                        var foundMatch = false;
                        foreach (var optionValue in unwrappedElement.SelectedOptionValues)
                        {
                            if (IsTextMatch(optionValue, value))
                            {
                                foundMatch = true;
                                break;
                            }
                        }

                        if (!foundMatch)
                        {
                            throw new FluentExpectFailedException("Expected SelectElement selected options to have at least one option with value of [{0}]. Selected option text values include [{1}]", value, string.Join(",", unwrappedElement.SelectedOptionValues));
                        }
                    }
                    else
                    {
                        if (!IsTextMatch(unwrappedElement.Value, value))
                        {
                            throw new FluentExpectFailedException("Expected SelectElement selected option value to be [{0}] but it was actually [{1}].", value, unwrappedElement.Text);
                        }
                    }
                }
                else
                {
                    if (!IsTextMatch(unwrappedElement.Value, value))
                    {
                        throw new FluentExpectFailedException("Expected element value to be [{0}] but it was actually [{1}].", value, unwrappedElement.Value);
                    }
                }
            });
        }

        public void Value(Func<IElement> element, Expression<Func<string, bool>> matchFunc)
        {
            this.commandProvider.Act(() =>
            {
                var compiledFunc = matchFunc.Compile();
                var unwrappedElement = element();
                if (unwrappedElement.IsText)
                {
                    if (!compiledFunc(unwrappedElement.Value))
                    {
                        throw new FluentExpectFailedException("Expected TextElement value to match expression [{0}] but it was actually [{1}].", matchFunc.ToExpressionString(), unwrappedElement.Value);
                    }
                }
                else if (unwrappedElement.IsSelect)
                {
                    if (unwrappedElement.IsMultipleSelect)
                    {
                        var foundMatch = false;
                        foreach (var optionValue in unwrappedElement.SelectedOptionValues)
                        {
                            if (compiledFunc(optionValue))
                            {
                                foundMatch = true;
                                break;
                            }
                        }

                        if (!foundMatch)
                        {
                            throw new FluentExpectFailedException("Expected SelectElement selected options to have at least one option with value matching expression[{0}]. Selected option values include [{1}]", matchFunc.ToExpressionString(), string.Join(",", unwrappedElement.SelectedOptionTextCollection));
                        }
                    }
                    else
                    {
                        if (!compiledFunc(unwrappedElement.Text))
                        {
                            throw new FluentExpectFailedException("Expected SelectElement selected option value to match expression [{0}] but it was actually [{1}].", matchFunc.ToExpressionString(), unwrappedElement.Value);
                        }
                    }
                }
                else
                {
                    if (!compiledFunc(unwrappedElement.Value))
                    {
                        throw new FluentExpectFailedException("Expected element value to match expression [{0}] but it was actually [{1}].", matchFunc.ToExpressionString(), unwrappedElement.Value);
                    }
                }
            });
        }
        #endregion

        public void Url(Uri expectedUrl)
        {
            this.commandProvider.Act(() => {
                if (!expectedUrl.ToString().Equals(this.commandProvider.Url.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new FluentExpectFailedException("Expected URL to match [{0}] but it was actually [{1}].", expectedUrl.ToString(), this.commandProvider.Url.ToString());
                }
            });
        }

        public void Url(Expression<Func<Uri, bool>> urlExpression)
        {
            this.commandProvider.Act(() =>
            {
                var compiledExpr = urlExpression.Compile();

                if (compiledExpr(this.commandProvider.Url) != true)
                {
                    throw new FluentExpectFailedException("Expected expression [{0}] to return true.", urlExpression.ToExpressionString());
                }
            });
        }

        #region Boolean / Throws
        public void True(Expression<Func<bool>> matchFunc)
        {
            this.commandProvider.Act(() =>
            {
                var compiledFunc = matchFunc.Compile();
                if (!compiledFunc())
                {
                    throw new FluentExpectFailedException("Expected expression [{0}] to return false.", matchFunc.ToExpressionString());
                }
            });
        }

        public void False(Expression<Func<bool>> matchFunc)
        {
            this.commandProvider.Act(() =>
            {
                var compiledFunc = matchFunc.Compile();
                if (compiledFunc())
                {
                    throw new FluentExpectFailedException("Expected expression [{0}] to return false.", matchFunc.ToExpressionString());
                }
            });
        }

        public void Throws(Expression<Action> matchAction)
        {
            this.commandProvider.Act(() =>
            {
                bool threwException = false;
                var compiledAction = matchAction.Compile();

                try
                {
                    compiledAction();
                }
                catch (FluentExpectFailedException)
                {
                    threwException = true;
                }

                if (!threwException)
                {
                    throw new FluentExpectFailedException("Expected expression [{0}] to throw an exception.", matchAction.ToExpressionString());
                }
            });
        }
        #endregion

        public void Exists(string selector)
        {
            this.commandProvider.Act(() =>
            {
                var unwrappedElement = this.commandProvider.Find(selector)() as IElement;
                if (unwrappedElement == null)
                {
                    throw new FluentExpectFailedException("Expected element matching selector [{0}] to exist.", selector);
                }
            });
        }
    }
}