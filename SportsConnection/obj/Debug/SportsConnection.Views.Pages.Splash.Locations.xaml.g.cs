// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.42000
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace SportsConnection {
    using System;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;
    
    
    public partial class Locations : global::Xamarin.Forms.ContentPage {
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::Xamarin.Forms.AbsoluteLayout pageContainer;
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::Xamarin.Forms.StackLayout mainContainer;
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::Xamarin.Forms.StackLayout mapContainer;
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::FFImageLoading.Forms.CachedImage btnAddLocation;
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::SportsConnection.MsgContainer msgContainer;
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::SportsConnection.NoConnectionContainer noConnectionContainer;
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private void InitializeComponent() {
            this.LoadFromXaml(typeof(Locations));
            pageContainer = this.FindByName <global::Xamarin.Forms.AbsoluteLayout>("pageContainer");
            mainContainer = this.FindByName <global::Xamarin.Forms.StackLayout>("mainContainer");
            mapContainer = this.FindByName <global::Xamarin.Forms.StackLayout>("mapContainer");
            btnAddLocation = this.FindByName <global::FFImageLoading.Forms.CachedImage>("btnAddLocation");
            msgContainer = this.FindByName <global::SportsConnection.MsgContainer>("msgContainer");
            noConnectionContainer = this.FindByName <global::SportsConnection.NoConnectionContainer>("noConnectionContainer");
        }
    }
}