﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities.Input;
using Microsoft.VisualStudio.LanguageServices.Implementation.GenerateType;
using Roslyn.Utilities;

namespace Microsoft.VisualStudio.IntegrationTest.Utilities.InProcess
{
    internal class GenerateTypeDialog_InProc : InProcComponent
    {
        private GenerateTypeDialog_InProc()
        {
        }

        public static GenerateTypeDialog_InProc Create()
            => new GenerateTypeDialog_InProc();

        public void VerifyOpen()
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                var cancellationToken = cancellationTokenSource.Token;
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var window = JoinableTaskFactory.Run(() => TryGetDialogAsync(cancellationToken));
                    if (window is null)
                    {
                        // Thread.Yield is insufficient; something in the light bulb must be relying on a UI thread
                        // message at lower priority than the Background priority used in testing.
                        WaitForApplicationIdle();
                        continue;
                    }

                    WaitForApplicationIdle();
                    return;
                }
            }
        }

        public void VerifyClosed()
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                var cancellationToken = cancellationTokenSource.Token;
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var window = JoinableTaskFactory.Run(() => TryGetDialogAsync(cancellationToken));
                    if (window is null)
                    {
                        return;
                    }

                    Thread.Yield();
                }
            }
        }

        public bool CloseWindow()
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                if (JoinableTaskFactory.Run(() => TryGetDialogAsync(cancellationTokenSource.Token)) is null)
                {
                    return false;
                }
            }

            ClickCancel();
            return true;
        }

        public void ClickOK()
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                JoinableTaskFactory.Run(() => ClickAsync(testAccessor => testAccessor.OKButton, cancellationTokenSource.Token));
            }
        }

        public void ClickCancel()
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                JoinableTaskFactory.Run(() => ClickAsync(testAccessor => testAccessor.CancelButton, cancellationTokenSource.Token));
            }
        }

        public void SetAccessibility(string accessibility)
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                JoinableTaskFactory.Run(async () =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationTokenSource.Token);
                    var dialog = await GetDialogAsync(cancellationTokenSource.Token);
                    Contract.ThrowIfFalse(await dialog.GetTestAccessor().AccessListComboBox.SimulateSelectItemAsync(JoinableTaskFactory, accessibility));
                });
            }
        }

        public void SetKind(string kind)
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                JoinableTaskFactory.Run(async () =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationTokenSource.Token);
                    var dialog = await GetDialogAsync(cancellationTokenSource.Token);
                    Contract.ThrowIfFalse(await dialog.GetTestAccessor().KindListComboBox.SimulateSelectItemAsync(JoinableTaskFactory, kind));
                });
            }
        }

        public void SetTargetProject(string projectName)
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                JoinableTaskFactory.Run(async () =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationTokenSource.Token);
                    var dialog = await GetDialogAsync(cancellationTokenSource.Token);
                    Contract.ThrowIfFalse(await dialog.GetTestAccessor().ProjectListComboBox.SimulateSelectItemAsync(JoinableTaskFactory, projectName));
                });
            }
        }

        public void SetTargetFileToNewName(string newFileName)
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                JoinableTaskFactory.Run(async () =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationTokenSource.Token);
                    var dialog = await GetDialogAsync(cancellationTokenSource.Token);
                    Contract.ThrowIfFalse(await dialog.GetTestAccessor().CreateNewFileRadioButton.SimulateClickAsync(JoinableTaskFactory));
                    Contract.ThrowIfFalse(await dialog.GetTestAccessor().CreateNewFileComboBox.SimulateSelectItemAsync(JoinableTaskFactory, newFileName, mustExist: false));
                });
            }
        }

        public void SetTargetFileToExisting(string existingFileName)
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                JoinableTaskFactory.Run(async () =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationTokenSource.Token);
                    var dialog = await GetDialogAsync(cancellationTokenSource.Token);
                    Contract.ThrowIfFalse(await dialog.GetTestAccessor().AddToExistingFileRadioButton.SimulateClickAsync(JoinableTaskFactory));
                    Contract.ThrowIfFalse(await dialog.GetTestAccessor().AddToExistingFileComboBox.SimulateSelectItemAsync(JoinableTaskFactory, existingFileName, mustExist: false));
                });
            }
        }

        public string[] GetNewFileComboBoxItems()
        {
            using (var cancellationTokenSource = new CancellationTokenSource(Helper.HangMitigatingTimeout))
            {
                return JoinableTaskFactory.Run(async () =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationTokenSource.Token);

                    var dialog = await GetDialogAsync(cancellationTokenSource.Token);
                    return dialog.GetTestAccessor().CreateNewFileComboBox.Items.Cast<string>().ToArray();
                });
            }
        }

        private async Task<GenerateTypeDialog> GetDialogAsync(CancellationToken cancellationToken)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: true, cancellationToken);
            return Application.Current.Windows.OfType<GenerateTypeDialog>().Single();
        }

        private async Task<GenerateTypeDialog> TryGetDialogAsync(CancellationToken cancellationToken)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: true, cancellationToken);
            return Application.Current.Windows.OfType<GenerateTypeDialog>().SingleOrDefault();
        }

        private async Task ClickAsync(Func<GenerateTypeDialog.TestAccessor, ButtonBase> buttonSelector, CancellationToken cancellationToken)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: true, cancellationToken);
            var dialog = await GetDialogAsync(cancellationToken);
            var button = buttonSelector(dialog.GetTestAccessor());
            Contract.ThrowIfFalse(await button.SimulateClickAsync(JoinableTaskFactory));
        }
    }
}
