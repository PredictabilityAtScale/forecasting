using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.Reflection;
using System.Xml;

namespace FocusedObjective.Simulation
{
    internal class ModelAudit
    {
        private SimulationData _data;
        private List<ModelComment> _comments = new List<ModelComment>();

        internal enum SeverityLevel
        {
            major = 100,
            important = 75,
            medium = 50,
            low = 25,
            none = 0
        }

        internal enum CommentType
        {
            accuracy,
            advice
        }

        internal class ModelComment
        {
            internal string Comment { get; set; }
            internal CommentType CommentType { get; set; }
            internal SeverityLevel CommentSeverity { get; set; }
            internal XElement Source { get; set; }
        }

        internal ModelAudit(SimulationData data)
        {
            _data = data;
        }

        internal XElement AsXml()
        {
            processRules();

            var comments = from c in _comments
                           let line = c.Source != null ? ((IXmlLineInfo)c.Source).LineNumber : 0
                           let pos = c.Source != null ? ((IXmlLineInfo)c.Source).LinePosition : 0
                           orderby (int)c.CommentSeverity descending
                           select new XElement("comment",
                               new XAttribute("level", c.CommentSeverity.ToString()),
                               new XAttribute("type", c.CommentType.ToString()),
                               new XAttribute("line", line),
                               new XAttribute("pos", pos),
                               c.Comment);

            return new XElement("modelAudit", 
                new XAttribute("score", _comments.Sum(c => (int)c.CommentSeverity)),
                    comments);
        }

        private void processRules()
        {
            // call all methods marked with the [ModelRule] attribute
            var methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where( m => m.GetCustomAttributes(typeof(ModelRuleAttribute), true).Any() );

            foreach (var method in methods)
                method.Invoke(this, new object[] { });
        }

        [ModelRule]
        private void Rule_NoDefects()
        {
            // models without defects are poor
            if (!_data.Setup.Defects.Any())
            {
                _comments.Add(new ModelComment
                {
                    CommentSeverity = SeverityLevel.major,
                    CommentType = CommentType.accuracy,
                    Comment = "No defect events defined in the model. Add one or more defect events to account for issues found during testing that cause rework."
                });
            }
        }

        [ModelRule]
        private void Rule_NoBlockingEvents()
        {
            if (!_data.Setup.BlockingEvents.Any())
            {
                _comments.Add(new ModelComment
                {
                    CommentSeverity = SeverityLevel.important,
                    CommentType = CommentType.accuracy,
                    Comment = "No blocking events defined in the model. Add one or more blocking events to account for delays in completing work one it is started.",
                });
            }
        }

        [ModelRule]
        private void Rule_NoAddedScopeEvents()
        {
            if (!_data.Setup.AddedScopes.Any())
            {
                _comments.Add(new ModelComment
                {
                    CommentSeverity = SeverityLevel.important,
                    CommentType = CommentType.accuracy,
                    Comment = "No added scope events defined in the model. Add one or more added scope events to account for work that is added over time. Some examples of added scope are: scope creep, split-stories, refactoring, architecture, deployment automation."
                });
            }
        }

        [ModelRule]
        private void Rule_NoDeliverables()
        {
            if (_data.Setup.Backlog.Deliverables.Count() <= 1)
            {
                _comments.Add(new ModelComment
                {
                    CommentSeverity = SeverityLevel.medium,
                    CommentType = CommentType.advice,
                    Comment = "Backlog is contained in only one deliverable. By having multiple deliverables segment your backlog you have the ability to perform analysis on one or with combinations of deliverable options. For example, forecast a date for must-have features to see if that division of backlog helps you hit a certain date."
                });
            }
        }

        [ModelRule]
        private void Rule_DefectHasNoColumnOverrides()
        {
            if (_data.Execute.SimulationType == SimulationTypeEnum.Kanban)
            {
                var query = _data.Setup.Defects.Where(d => !d.Columns.Any());

                foreach (var defect in query)
                {
                    _comments.Add(new ModelComment
                    {
                        CommentSeverity = SeverityLevel.low,
                        CommentType = CommentType.accuracy,
                        Source = defect.Source,
                        Comment = string.Format(
                          
                               "Defect '{0}' has no column cycle-time overrides. Often (almost always) rework due to a defect takes less time than the original work, and especially so in some columns. For example, the re-test time should be less than the testing columns general cycle-time.",
                               defect.Name)
                    });
                }
            }
        }

        [ModelRule]
        private void Rule_BacklogEntryHasNoColumnOverrides()
        {
            if (_data.Execute.SimulationType == SimulationTypeEnum.Kanban)
            {
                var query = 
                    _data.Setup.Backlog.Deliverables.SelectMany(d => d.CustomBacklog)
                    .Where(c => 
                        (c.Count > 1) &&
                            ((c.PercentageLowBound == 0.0 && c.PercentageHighBound == 100.0) && 
                             !c.Columns.Any()));

                foreach (var backlog in query)
                {
                    _comments.Add(new ModelComment
                    {
                        CommentSeverity = SeverityLevel.medium,
                        CommentType = CommentType.accuracy,
                        Source = backlog.Source,
                        Comment = string.Format(
                               "Backlog entry '{0}' has no percentage override or column cycle-time override defined. The major reason for defining a backlog entry group is so that custom cycle-times can be applied through a percentage range, or through custom column overrides.",
                               backlog.Name)
                    });
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class ModelRuleAttribute : Attribute
    {
    }
}
