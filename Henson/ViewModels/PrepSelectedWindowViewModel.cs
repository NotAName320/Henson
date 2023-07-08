﻿/*
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
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MsBox.Avalonia.Enums;

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
        private readonly NsClient _client;

        /// <summary>
        /// The list of all nations loaded by Henson.
        /// </summary>
        private readonly List<NationGridViewModel> _nations;

        /// <summary>
        /// A list of nations selected by the user.
        /// </summary>
        private readonly List<NationLoginViewModel> _selectedNations;

        /// <summary>
        /// The current index that the user is on.
        /// </summary>
        private int _loginIndex = 0;

        /// <summary>
        /// The number of nations successfully prepped without errors (e.g. moved region successfully).
        /// </summary>
        private int _prepSuccesses = 0;

        /// <summary>
        /// The current chk of the logged in nation.
        /// </summary>
        private string _currentChk = "";

        /// <summary>
        /// The current Local ID of the logged in nation.
        /// </summary>
        private string _currentLocalId = "";

        /// <summary>
        /// The current Pin of the logged in nation.
        /// </summary>
        private string _currentPin = "";

        /// <summary>
        /// The names of all the logins that failed.
        /// </summary>
        private readonly StringBuilder _failedLogins = new();

        /// <summary>
        /// The text on the button.
        /// </summary>
        public string ButtonText
        {
            get => _buttonText;
            set
            {
                this.RaiseAndSetIfChanged(ref _buttonText, value);
            }
        }
        private string _buttonText = "Login";

        /// <summary>
        /// Whether or not the Apply WA? checkox is checked.
        /// </summary>
        public bool AlsoApplyWa
        {
            get => _alsoApplyWa;
            set
            {
                this.RaiseAndSetIfChanged(ref _alsoApplyWa, value);
            }
        }
        private bool _alsoApplyWa = true;

        /// <summary>
        /// The value displayed by the current login text block in the window.
        /// </summary>
        public string CurrentLogin
        {
            get => _currentLogin;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentLogin, value);
            }
        }
        private string _currentLogin = "";

        /// <summary>
        /// The text displayed in the footer of the window.
        /// </summary>
        public string FooterText
        {
            get => _footerText;
            set
            {
                this.RaiseAndSetIfChanged(ref _footerText, value);
            }
        }
        private string _footerText = "Click Login to start.";

        /// <summary>
        /// Boolean that controls the enabling and disabling of buttons that send requests
        /// to ensure compliance with API.
        /// </summary>
        public bool ButtonsEnabled
        {
            get => _buttonsEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _buttonsEnabled, value);
            }
        }
        private bool _buttonsEnabled = true;

        /// <summary>
        /// The log4net logger. It will emit messages as from PrepSelectedWindowViewModel.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        /// Constructs a new <c>PrepSelectedViewModel</c>.
        /// </summary>
        /// <param name="nations">The list of nations from the parent <c>MainWindowViewModel</c>.</param>
        /// <param name="client">The client from the parent <c>MainWindowViewModel</c>.</param>
        /// <param name="target">The prefilled region from the target box.</param>
        public PrepSelectedWindowViewModel(List<NationGridViewModel> nations, NsClient client, string target)
        {
            _nations = nations;
            _selectedNations = _nations.Where(x => x.Checked && !x.Locked).Select(x => new NationLoginViewModel(x.Name, x.Pass)).ToList();
            _client = client;
            TargetRegion = target;

            ActionButtonCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(_loginIndex == _selectedNations.Count)
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

                NationLoginViewModel currentNation = _selectedNations[_loginIndex];

                ButtonsEnabled = false;
                await Task.Delay(100);
                switch(_buttonText)
                {
                    case "Login":
                        var (chk, localId, pin, region) = await _client.Login(currentNation) ?? default;
                        if(chk != null)
                        {
                            _currentChk = chk;
                            _currentLocalId = localId;
                            _currentPin = pin;
                            CurrentLogin = currentNation.Name;

                            //cant be bothered to change nations type to NationGridViewModel
                            //have this dumb LINQ instead
                            if(region!.ToLower() != nations.First(x => x.Name == currentNation.Name).Region.ToLower())
                            {
                                _nations.First(x => x.Name == currentNation.Name).Region = region;
                                DbClient.ExecuteNonQuery($"UPDATE nations SET region = '{region}' WHERE name = '{currentNation.Name}'");
                            }

                            FooterText = $"Logged in to {currentNation.Name}.";
                            ButtonText = AlsoApplyWa ? "Apply WA" : "Move to JP";
                        }
                        else
                        {
                            FooterText = $"Login to {currentNation.Name} failed.";
                            AddToFailedLogins(currentNation.Name);
                            _loginIndex++;
                        }
                        break;
                    case "Apply WA":
                        if(await _client.ApplyWa(_currentChk, _currentPin))
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
                            _loginIndex++;
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

                        if(_nations.Where(x => x.Name == currentNation.Name).First().Region.ToLower() == TargetRegion.ToLower())
                        {
                            FooterText = $"{currentNation.Name} already in {TargetRegion}!";
                        }
                        else if(!await _client.MoveToJp(TargetRegion, _currentLocalId, _currentPin))
                        {
                            FooterText = $"Moving {currentNation.Name} failed.";
                            AddToFailedLogins(currentNation.Name);
                        }
                        else
                        {
                            FooterText = $"Moved {currentNation.Name} to {TargetRegion}.";
                            _nations.First(x => x.Name == currentNation.Name).Region = char.ToUpper(TargetRegion[0]) + TargetRegion[1..];
                            DbClient.ExecuteNonQuery($"UPDATE nations SET region = '{char.ToUpper(TargetRegion[0]) + TargetRegion[1..]}' WHERE name = '{currentNation.Name}'");
                            _prepSuccesses++;
                        }
                        ButtonText = "Login";
                        _loginIndex++;
                        if(_loginIndex == _selectedNations.Count)
                        {
                            FooterText += $" {_prepSuccesses}/{_selectedNations.Count} prepped successfully!";
                        }
                        break;
                }

                if(_loginIndex == _selectedNations.Count && _failedLogins.Length != 0)
                {
                    _failedLogins.Remove(_failedLogins.Length - 2, 2);
                    MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                    {
                        ContentTitle = "Some Logins Failed",
                        ContentMessage = $"The following nations failed to be prepped ({_selectedNations.Count-_prepSuccesses}/{_selectedNations.Count}):" +
                        $"\n{_failedLogins}\n\nCheck the log for more info.",
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
            Log.Error($"Prepping {loginName} failed");
            if(_failedLogins.ToString().Split("\n").Last().Length + loginName.Length + 2 > 75)
            {
                _failedLogins.Append('\n');
            }
            _failedLogins.Append(loginName + ", ");
        }
    }
}
