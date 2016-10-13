﻿/* 
 * Copyright (c) 2016, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Model;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Collections.Generic;
using Hl7.Fhir.Specification.Navigation;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.FluentPath;
using Hl7.ElementModel;
using Hl7.FluentPath;
using Hl7.Fhir.Validation;

namespace Hl7.Fhir.Specification.Tests
{
    [TestClass]
#if PORTABLE45
	public class PortableOutcomeReportingTest
#else
    public class OutcomeReportingTest
#endif
    {
        [TestInitialize]
        public void Setup()
        {
            OperationOutcome level0 = new OperationOutcome();

            level0.AddIssue(Issue.CONTENT_ELEMENT_HAS_INCORRECT_TYPE.ToIssueComponent("A test error at level 0", "Patient"));

            OperationOutcome level1 = new OperationOutcome();

            Patient p = new Patient();
            p.Active = true;
            _location = new PocoNavigator(p);
            _location.MoveToFirstChild();

            level1.AddIssue(Issue.PROFILE_ELEMENTDEF_CARDINALITY_MISSING.ToIssueComponent("A test warning at level 1", _location));

            OperationOutcome level2 = new OperationOutcome();

            level2.AddIssue(Issue.UNSUPPORTED_CONSTRAINT_WITHOUT_FLUENTPATH.ToIssueComponent("A test warning at level 2", "Patient.active[0].id[0]"));
            level2.AddIssue(Issue.CONTENT_ELEMENT_MUST_MATCH_TYPE.ToIssueComponent("Another test error at level 2", "Patient.active[0].id[0]"));

            level1.Include(level2);

            level0.Include(level1);

            level0.AddIssue(Issue.PROFILE_ELEMENTDEF_IS_EMPTY.ToIssueComponent("A test warning at level 0", "Patient"));

            _report = level0;
        }

        private OperationOutcome _report;
        private IElementNavigator _location;

        [TestMethod]
        public void IssueHierarchy()
        {
            Assert.AreEqual(5, _report.Issue.Count());
            Assert.AreEqual(2, _report.AtLevel(0).Count());
            Assert.AreEqual(1, _report.AtLevel(1).Count());
            Assert.AreEqual(2, _report.AtLevel(2).Count());

            CollectionAssert.AreEquivalent(new int[] { 0, 1, 2, 2, 0 }, _report.Issue.Select(i=>i.GetHierarchyLevel()).ToArray());
        }

        [TestMethod]
        public void IssueCategorization()
        {
            Assert.AreEqual(2, _report.ListErrors().Count());
            Assert.AreEqual(3, _report.Where(severity: OperationOutcome.IssueSeverity.Warning).Count());
            Assert.AreEqual(2, _report.Where(type: OperationOutcome.IssueType.BusinessRule).Count());
            Assert.AreEqual(1, _report.Where(issueCode: Issue.PROFILE_ELEMENTDEF_CARDINALITY_MISSING.Code).Count());
            Assert.AreEqual(1, _report.Where(severity: OperationOutcome.IssueSeverity.Warning, type: OperationOutcome.IssueType.BusinessRule, issueCode: 2008).Count());
            Assert.AreEqual(0, _report.Where(severity: OperationOutcome.IssueSeverity.Error, type: OperationOutcome.IssueType.BusinessRule, issueCode: 2008).Count());
        }

        [TestMethod]
        public void IssueLocation()
        {
            Assert.AreEqual(1, _report.ErrorsAt("Patient.active[0].id[0]").Count());
            Assert.AreEqual(0, _report.ErrorsAt(_location).Count());
            Assert.AreEqual(1, _report.IssuesAt(_location).Count());
            Assert.AreEqual(1, _report.ErrorsAt("Patient").Count());
        }
    }

}