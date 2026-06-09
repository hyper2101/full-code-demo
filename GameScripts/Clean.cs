using System.IO;
using System.Text.RegularExpressions;

string file1 = @"C:\Users\Administrator\Documents\GitHub\full-code-demo\GameScripts\Core\WorldManager.cs";
string content1 = File.ReadAllText(file1);
content1 = Regex.Replace(content1, @"public IEnumerator FinishDemand\(Demand demand, DemandEvent demandEvent\).*?yield return this\.currentAnimationRoutine;\s*\}", "", RegexOptions.Singleline);
File.WriteAllText(file1, content1);

string file2 = @"C:\Users\Administrator\Documents\GitHub\full-code-demo\GameScripts\Legacy\Stacklands\Quests\AllQuests.cs";
string content2 = File.ReadAllText(file2);
content2 = Regex.Replace(content2, @"public Quest Have3DemandsSucceeded\(QuestGroup group\).*?return WorldManager\.instance\.CurrentRunVariables\.PreviousDemandEvents\.Count<DemandEvent>\(\(DemandEvent x\) => x\.Successful\) >= 3;\s*\}", "", RegexOptions.Singleline);
content2 = Regex.Replace(content2, @"public Quest Have5DemandsSucceeded\(QuestGroup group\).*?return WorldManager\.instance\.CurrentRunVariables\.PreviousDemandEvents\.Count<DemandEvent>\(\(DemandEvent x\) => x\.Successful\) >= 5;\s*\}", "", RegexOptions.Singleline);
content2 = Regex.Replace(content2, @"public Quest Have8DemandsSucceeded\(QuestGroup group\).*?return WorldManager\.instance\.CurrentRunVariables\.PreviousDemandEvents\.Count<DemandEvent>\(\(DemandEvent x\) => x\.Successful\) >= 8;\s*\}", "", RegexOptions.Singleline);
File.WriteAllText(file2, content2);
