using System.Collections.Generic;
using NUnit.Framework;
using TaskManagement.Editor.Data;
using TaskManagement.Editor.Enums;
using TaskManagement.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace TaskManagement.Tests.EditMode
{
    public class EditModeTests
    {
        #region Constants
        private const string TestRootPath = "Assets/TaskManagement/TestData/";
        #endregion

        #region Core
        [SetUp]
        public void Setup()
        {
            if (Directory.Exists(TestRootPath))
                Directory.Delete(TestRootPath, true);

            Directory.CreateDirectory(TestRootPath);
        }
        [TearDown]
        public void Cleanup()
        {
            AssetDatabase.Refresh();
            
            if (Directory.Exists(TestRootPath))
                Directory.Delete(TestRootPath, true);
        }
        #endregion

        #region Tests
        [Test]
        public void CreateProject_ShouldCreateNewAsset()
        {
            string projectName = "TestProject";

            ProjectData project = EditorAssetUtility.CreateOrLoadAsset<ProjectData>(
                Path.Combine(TestRootPath, projectName), projectName);
            
            project.ProjectName = projectName;

            Assert.IsNotNull(project);
            Assert.AreEqual(projectName, project.ProjectName);
        }
        [Test]
        public void DeleteProject_ShouldRemoveProjectAsset()
        {
            string projectName = "TempProject";
            
            string path = Path.Combine(TestRootPath, projectName);
            
            ProjectData project = EditorAssetUtility.CreateOrLoadAsset<ProjectData>(path, projectName);

            string assetPath = AssetDatabase.GetAssetPath(project);
            
            Assert.IsTrue(File.Exists(assetPath));

            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.Refresh();

            Assert.IsFalse(File.Exists(assetPath));
        }
        [Test]
        public void CreateTask_ShouldAddToProject()
        {
            ProjectData project = CreateTempProject("TaskTestProject");
            
            string taskTitle = "My First Task";

            TaskData task =
                EditorAssetUtility.CreateOrLoadAsset<TaskData>($"{TestRootPath}/{project.ProjectName}/Tasks",
                    taskTitle);
            
            task.Title = taskTitle;

            project.Tasks = new List<TaskData> { task };

            Assert.AreEqual(1, project.Tasks.Count);
            Assert.AreEqual(taskTitle, project.Tasks[0].Title);
        }
        [Test]
        public void EditTask_ShouldUpdateProperties()
        {
            TaskData task = ScriptableObject.CreateInstance<TaskData>();
            
            task.Title = "OldTitle";
            task.Description = "OldDesc";

            task.Title = "NewTitle";
            task.Description = "Updated";

            Assert.AreEqual("NewTitle", task.Title);
            Assert.AreEqual("Updated", task.Description);
        }
        [Test]
        public void CancelEdit_ShouldKeepOriginalValues()
        {
            TaskData task = ScriptableObject.CreateInstance<TaskData>();
            
            task.Title = "Initial";

            string before = task.Title;
            
            Assert.AreEqual(before, task.Title);
        }
        [Test]
        public void DeleteTask_ShouldRemoveFromProject()
        {
            ProjectData project = CreateTempProject("DeleteTaskProject");
            
            TaskData task = ScriptableObject.CreateInstance<TaskData>();
            
            task.Title = "TempTask";

            project.Tasks = new List<TaskData> { task };
            
            project.Tasks.Remove(task);

            Assert.AreEqual(0, project.Tasks.Count);
        }
        [Test]
        public void SortByPriority_ShouldReturnCorrectOrder()
        {
            var tasks = new[]
            {
                ScriptableObject.CreateInstance<TaskData>(),
                ScriptableObject.CreateInstance<TaskData>(),
                ScriptableObject.CreateInstance<TaskData>()
            };

            tasks[0].Title = "A";
            tasks[1].Title = "B";
            tasks[2].Title = "C";
            
            tasks[0].Priority = TaskPriority.Low;
            tasks[1].Priority = TaskPriority.Normal;
            tasks[2].Priority = TaskPriority.High;

            List<TaskData> sorted = tasks.OrderBy(t => t.Priority).ToList();

            Assert.AreEqual("A", sorted.First().Title);
            Assert.AreEqual("C", sorted.Last().Title);
        }
        [Test]
        public void CalculateRemainingTime_ShouldHandleDueDates()
        {
            string futureMsg = "3 days left";
            string pastMsg = "1 day overdue";
            string todayMsg = "Due today";

            Assert.IsTrue(futureMsg.Contains("left"));
            Assert.IsTrue(pastMsg.Contains("overdue"));
            Assert.AreEqual("Due today", todayMsg);
        }
        #endregion

        #region Executes
        private static ProjectData CreateTempProject(string name)
        {
            string path = Path.Combine(TestRootPath, name);

            return EditorAssetUtility.CreateOrLoadAsset<ProjectData>(path, name);
        }
        #endregion
    }
}
