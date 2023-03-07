using Henson.Models;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Henson.ViewModels
{
    public class PrepSelectedViewModel : ViewModelBase
    {
        private NsClient Client { get; }
        private List<NationLoginViewModel> Nations { get; set; }
        private int LoginIndex { get; set; } = 0;
        private string CurrentChk { get; set; } = "";
        private string CurrentLocalID { get; set; } = "";
        public ICommand ActionButtonCommand { get; }
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();


        public PrepSelectedViewModel(List<NationLoginViewModel> nations, NsClient client)
        {
            Nations = nations;
            Client = client;

            ActionButtonCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (LoginIndex == Nations.Count)
                {
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Logins complete",
                        ContentMessage = $"All nations have been prepped. Please close the window now.",
                        Icon = Icon.Error,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;
                }

                NationLoginViewModel currentNation = Nations[LoginIndex];

                switch(buttonText)
                {
                    case "Login":
                        var (chk, localId) = Client.Login(currentNation) ?? default;
                        if(chk != null)
                        {
                            CurrentChk = chk;
                            CurrentLocalID = localId;
                            CurrentLogin = currentNation.Name;

                            ButtonText = AlsoApplyWA ? "Apply WA" : "Move to JP";
                        }
                        else
                        {
                            MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                            {
                                ContentTitle = "Login Failed",
                                ContentMessage = $"Logging in to {currentNation.Name} failed. Going to the next nation.",
                                Icon = Icon.Error,
                            });
                            await MessageBoxDialog.Handle(dialog);
                            LoginIndex++;
                        }
                        break;
                    case "Apply WA":
                        if(Client.ApplyWA(CurrentChk))
                        {
                            ButtonText = "Move to JP";
                        }
                        else
                        {
                            MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                            {
                                ContentTitle = "Login Failed",
                                ContentMessage = $"Applying to WA on {currentNation.Name} failed. Going to the next nation.",
                                Icon = Icon.Error,
                            });
                            await MessageBoxDialog.Handle(dialog);
                            
                            ButtonText = "Login";
                            CurrentLogin = "";
                            LoginIndex++;
                        }
                        break;
                    case "Move to JP":
                        if(TargetRegion == "")
                        {
                            MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                            {
                                ContentTitle = "Target Region Not Set",
                                ContentMessage = "Please set a target region.",
                                Icon = Icon.Error,
                            });
                            await MessageBoxDialog.Handle(dialog);
                            return;
                        }

                        if(!Client.MoveToJP(TargetRegion, CurrentLocalID))
                        {
                            MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                            {
                                ContentTitle = "Moving Nation To Region Failed",
                                ContentMessage = $"Moving the nation {currentNation.Name} to region {TargetRegion} failed.",
                                Icon = Icon.Error,
                            });
                            await MessageBoxDialog.Handle(dialog);
                            return;
                        }

                        ButtonText = "Login";
                        LoginIndex++;
                        
                        break;
                }
            });
        }

        private string buttonText = "Login";
        public string ButtonText
        {
            get => buttonText;
            set
            {
                this.RaiseAndSetIfChanged(ref buttonText, value);
            }
        }

        private bool alsoApplyWA = true;
        public bool AlsoApplyWA
        {
            get => alsoApplyWA;
            set
            {
                this.RaiseAndSetIfChanged(ref alsoApplyWA, value);
            }
        }

        private string currentLogin = "";
        public string CurrentLogin
        {
            get => currentLogin;
            set
            {
                this.RaiseAndSetIfChanged(ref currentLogin, value);
            }
        }

        private string targetRegion = "";
        public string TargetRegion
        {
            get => targetRegion;
            set
            {
                this.RaiseAndSetIfChanged(ref targetRegion, value);
            }
        }
    }
}
