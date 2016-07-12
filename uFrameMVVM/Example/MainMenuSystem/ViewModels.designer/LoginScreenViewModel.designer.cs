// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Example {
    using Example;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using uFrame.IOC;
    using uFrame.Kernel;
    using uFrame.Kernel.Serialization;
    using uFrame.MVVM;
    using uFrame.MVVM.Bindings;
    using uFrame.MVVM.ViewModels;
    using UniRx;
    using UnityEngine;
    
    
    public partial class LoginScreenViewModelBase : SubScreenViewModel {
        
        private P<String> _UsernameProperty;
        
        private P<String> _PasswordProperty;
        
        private Signal<LoginCommand> _Login;
        
        public LoginScreenViewModelBase(uFrame.Kernel.IEventAggregator aggregator) : 
                base(aggregator) {
        }
        
        public virtual P<String> UsernameProperty {
            get {
                return _UsernameProperty;
            }
            set {
                _UsernameProperty = value;
            }
        }
        
        public virtual P<String> PasswordProperty {
            get {
                return _PasswordProperty;
            }
            set {
                _PasswordProperty = value;
            }
        }
        
        public virtual String Username {
            get {
                return UsernameProperty.Value;
            }
            set {
                UsernameProperty.Value = value;
            }
        }
        
        public virtual String Password {
            get {
                return PasswordProperty.Value;
            }
            set {
                PasswordProperty.Value = value;
            }
        }
        
        public virtual Signal<LoginCommand> Login {
            get {
                return _Login;
            }
            set {
                _Login = value;
            }
        }
        
        public override void Bind() {
            base.Bind();
            this.Login = new Signal<LoginCommand>(this);
            _UsernameProperty = new P<String>(this, "Username");
            _PasswordProperty = new P<String>(this, "Password");
        }
        
        public virtual void ExecuteLogin() {
            this.Login.OnNext(new LoginCommand());
        }
        
        public override void Read(uFrame.Kernel.Serialization.ISerializerStream stream) {
            base.Read(stream);
            this.Username = stream.DeserializeString("Username");;
            this.Password = stream.DeserializeString("Password");;
        }
        
        public override void Write(uFrame.Kernel.Serialization.ISerializerStream stream) {
            base.Write(stream);
            stream.SerializeString("Username", this.Username);
            stream.SerializeString("Password", this.Password);
        }
        
        protected override void FillCommands(System.Collections.Generic.List<uFrame.MVVM.ViewModels.ViewModelCommandInfo> list) {
            base.FillCommands(list);
            list.Add(new ViewModelCommandInfo("Login", Login) { ParameterType = typeof(void) });
        }
        
        protected override void FillProperties(System.Collections.Generic.List<uFrame.MVVM.ViewModels.ViewModelPropertyInfo> list) {
            base.FillProperties(list);
            // PropertiesChildItem
            list.Add(new ViewModelPropertyInfo(_UsernameProperty, false, false, false, false));
            // PropertiesChildItem
            list.Add(new ViewModelPropertyInfo(_PasswordProperty, false, false, false, false));
        }
    }
    
    public partial class LoginScreenViewModel {
        
        public LoginScreenViewModel(uFrame.Kernel.IEventAggregator aggregator) : 
                base(aggregator) {
        }
    }
}