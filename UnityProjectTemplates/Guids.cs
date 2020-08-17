using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityProjectTemplates
{
    static class Guids
    {
        public const string GuidCSharpProjectString = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";
        public static readonly Guid GuidCSharpProject = new Guid(GuidCSharpProjectString);

        public const string GuidUnityProjectTemplateCommandPackageString = "59e25f6f-99d2-4be2-bf04-8ec839c7665a";
        public static readonly Guid GuidUnityProjectTemplateCommandPackage = new Guid(GuidUnityProjectTemplateCommandPackageString);

        public const string GuidUnityNewScriptCmdSetString = "413895c8-48aa-4630-b907-c67ee52c0368";
        public static readonly Guid GuidUnityNewScriptCmdSet = new Guid(GuidUnityNewScriptCmdSetString);

        public const string GuidUnityNewTestCmdSetString = "dea55eda-7d0c-4a18-9bd1-5d91d7636a83";
        public static readonly Guid GuidUnityNewTestCmdSet = new Guid(GuidUnityNewTestCmdSetString);

        public const string GuidUnityNewShaderCmdSetString = "c1325a24-e2d0-4186-92f6-0ba858a50f17";
        public static readonly Guid GuidUnityNewShaderCmdSet = new Guid(GuidUnityNewShaderCmdSetString);

        public const string GuidUnityTemplatesUIContext = "B83C6785-A21F-493B-B349-4168F63DE01E";
    }
}
