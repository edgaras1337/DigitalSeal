﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DigitalSeal.Core.Resources.Services {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class SharedResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SharedResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DigitalSeal.Core.Resources.Services.SharedResources", typeof(SharedResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No documents were selected.
        /// </summary>
        public static string General_MinLength_Documents {
            get {
                return ResourceManager.GetString("General.MinLength.Documents", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No organizations were selected.
        /// </summary>
        public static string General_MinLength_Organizations {
            get {
                return ResourceManager.GetString("General.MinLength.Organizations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No parties were selected.
        /// </summary>
        public static string General_MinLength_Parties {
            get {
                return ResourceManager.GetString("General.MinLength.Parties", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Document deadline is requried.
        /// </summary>
        public static string General_Required_DocDeadline {
            get {
                return ResourceManager.GetString("General.Required.DocDeadline", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Document name is required.
        /// </summary>
        public static string General_Required_DocName {
            get {
                return ResourceManager.GetString("General.Required.DocName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No file selected.
        /// </summary>
        public static string General_Required_NoFileSelected {
            get {
                return ResourceManager.GetString("General.Required.NoFileSelected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Organization name is required.
        /// </summary>
        public static string General_Required_OrgName {
            get {
                return ResourceManager.GetString("General.Required.OrgName", resourceCulture);
            }
        }
    }
}
