/*
View Model representing Prep Selected Window
Copyright (C) 2023 NotAName320

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using Henson.Models;
using log4net;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Henson.ViewModels
{
    public class PrepSelectedWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// The contents of the Target Region text box.
        /// </summary>
        public string TargetRegion { get; set; }

        /// <summary>
        /// The command fired when pressing the button.
        /// </summary>
        public ICommand ActionButtonCommand { get; }

        /// <summary>
        /// This interaction opens a MessageBox.Avalonia window with params given by the constructed ViewModel.
        /// I should really create a common class for these lmao
        /// </summary>
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();

        /// <summary>
        /// An object storing the UserAgent and using it to make requests to NationStates via both API and site.
        /// </summary>
        private NsClient Client { get; }

        /// <summary>
        /// The list of all nations loaded by Henson.
        /// </summary>
        private readonly List<NationGridViewModel> Nations;

        /// <summary>
        /// A list of nations selected by the user.
        /// </summary>
        private readonly List<NationLoginViewModel> SelectedNations;

        /// <summary>
        /// The current index that the user is on.
        /// </summary>
        private int LoginIndex = 0;

        /// <summary>
        /// The number of nations successfully prepped without errors (e.g. moved region successfully).
        /// </summary>
        private int PrepSuccesses = 0;

        /// <summary>
        /// The current chk of the logged in nation.
        /// </summary>
        private string CurrentChk = "";

        /// <summary>
        /// The current Local ID of the logged in nation.
        /// </summary>
        private string CurrentLocalID = "";

        /// <summary>
        /// The current Pin of the logged in nation.
        /// </summary>
        private string CurrentPin = "";

        /// <summary>
        /// The names of all the logins that failed.
        /// </summary>
        private StringBuilder FailedLogins = new();

        /// <summary>
        /// The text on the button.
        /// </summary>
        public string ButtonText
        {
            get => buttonText;
            set
            {
                this.RaiseAndSetIfChanged(ref buttonText, value);
            }
        }
        private string buttonText = "Login";

        /// <summary>
        /// Whether or not the Apply WA? checkox is checked.
        /// </summary>
        public bool AlsoApplyWA
        {
            get => alsoApplyWA;
            set
            {
                this.RaiseAndSetIfChanged(ref alsoApplyWA, value);
            }
        }
        private bool alsoApplyWA = true;

        /// <summary>
        /// The value displayed by the current login text block in the window.
        /// </summary>
        public string CurrentLogin
        {
            get => currentLogin;
            set
            {
                this.RaiseAndSetIfChanged(ref currentLogin, value);
            }
        }
        private string currentLogin = "";

        /// <summary>
        /// The text displayed in the footer of the window.
        /// </summary>
        public string FooterText
        {
            get => footerText;
            set
            {
                this.RaiseAndSetIfChanged(ref footerText, value);
            }
        }
        private string footerText = "Click Login to start.";

        /// <summary>
        /// Boolean that controls the enabling and disabling of buttons that send requests
        /// to ensure compliance with API.
        /// </summary>
        public bool ButtonsEnabled
        {
            get => buttonsEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref buttonsEnabled, value);
            }
        }
        private bool buttonsEnabled = true;

        /// <summary>
        /// The log4net logger. It will emit messages as from PrepSelectedWindowViewModel.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        /// Constructs a new <c>PrepSelectedViewModel</c>.
        /// </summary>
        /// <param name="nations">The list of nations from the parent <c>MainWindowViewModel</c>.</param>
        /// <param name="client">The client from the parent <c>MainWindowViewModel</c>.</param>
        /// <param name="target">The prefilled region from the target box.</param>
        public PrepSelectedWindowViewModel(List<NationGridViewModel> nations, NsClient client, string target)
        {
            Nations = nations;
            SelectedNations = Nations.Where(x => x.Checked && !x.Locked).Select(x => new NationLoginViewModel(x.Name, x.Pass)).ToList();
            Client = client;
            TargetRegion = target;

            ActionButtonCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(LoginIndex == SelectedNations.Count)
                {
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Logins Complete",
                        ContentMessage = $"All nations have been prepped. Please close the window now.",
                        Icon = Icon.Info,
                    });
                    await MessageBoxDialog.Handle(dialog);
                    return;
                }

                NationLoginViewModel currentNation = SelectedNations[LoginIndex];

                ButtonsEnabled = false;
                await Task.Delay(100);
                switch(buttonText)
                {
                    case "Login":
                        var (chk, localId, pin) = await Client.Login(currentNation) ?? default;
                        if(chk != null)
                        {
                            CurrentChk = chk;
                            CurrentLocalID = localId;
                            CurrentPin = pin;
                            CurrentLogin = currentNation.Name;

                            FooterText = $"Logged in to {currentNation.Name}.";
                            ButtonText = AlsoApplyWA ? "Apply WA" : "Move to JP";
                        }
                        else
                        {
                            FooterText = $"Login to {currentNation.Name} failed.";
                            AddToFailedLogins(currentNation.Name);
                            LoginIndex++;
                        }
                        break;
                    case "Apply WA":
                        if(await Client.ApplyWA(CurrentChk, CurrentPin))
                        {
                            ButtonText = "Move to JP";
                            FooterText = $"Sent WA application on {currentNation.Name}.";
                        }
                        else
                        {
                            FooterText = $"{currentNation.Name} WA App failed.";
                            AddToFailedLogins(currentNation.Name);

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

                        if(!await Client.MoveToJP(TargetRegion, CurrentLocalID, CurrentPin))
                        {
                            FooterText = $"Moving {currentNation.Name} failed.";
                            AddToFailedLogins(currentNation.Name);
                        }
                        else
                        {
                            FooterText = $"Moved {currentNation.Name} to {TargetRegion}.";
                            Nations.Where(x => x.Name == currentNation.Name).First().Region = TargetRegion;
                            PrepSuccesses++;
                        }
                        ButtonText = "Login";
                        LoginIndex++;
                        if(LoginIndex == SelectedNations.Count)
                        {
                            FooterText += $" {PrepSuccesses}/{SelectedNations.Count} prepped successfully!";
                        }
                        break;
                }

                if(LoginIndex == SelectedNations.Count && FailedLogins.Length != 0)
                {
                    FailedLogins.Remove(FailedLogins.Length - 2, 2);
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Some Logins Failed",
                        ContentMessage = $"The following nations failed to be prepped ({SelectedNations.Count-PrepSuccesses}/{SelectedNations.Count}):" +
                        $"\n{FailedLogins}\n\nCheck the log for more info.",
                        Icon = Icon.Warning,
                    });
                    await MessageBoxDialog.Handle(dialog);

                }

                ButtonsEnabled = true;
            });
        }

        /// <summary>
        /// Adds a name to the failed logins and wraps the string around if necessary.
        /// </summary>
        /// <param name="loginName">The login name to be added.</param>
        private void AddToFailedLogins(string loginName)
        {
            log.Error($"Prepping {loginName} failed");
            if(FailedLogins.ToString().Split("\n").Last().Length + loginName.Length + 2 > 75)
            {
                FailedLogins.Append('\n');
            }
            FailedLogins.Append(loginName + ", ");
        }
    }
}
