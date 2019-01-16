/***********************************************
 * CONFIDENTIAL AND PROPRIETARY 
 * 
 * The source code and other information contained herein is the confidential and exclusive property of
 * ZIH Corp. and is subject to the terms and conditions in your end user license agreement.
 * This source code, and any other information contained herein, shall not be copied, reproduced, published, 
 * displayed or distributed, in whole or in part, in any medium, by any means, for any purpose except as
 * expressly permitted under such license agreement.
 * 
 * Copyright ZIH Corp. 2018
 * 
 * ALL RIGHTS RESERVED
 ***********************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XamarinPrintStation {
    public class FormatViewModel : ViewModelBase {

        private int databaseId;
        private string driveLetter;
        private string name;
        private string extension;
        private string content;
        private FormatSource source;
        private bool isDeleting = false;
        private bool isSaving = false;

        public int DatabaseId {
            get => databaseId;
            set {
                databaseId = value;
                OnPropertyChanged();
            }
        }

        public string DriveLetter {
            get => driveLetter;
            set {
                driveLetter = value;
                OnPropertyChanged();
            }
        }

        public string Name {
            get => name;
            set {
                name = value;
                OnPropertyChanged();
            }
        }

        public string Extension {
            get => extension;
            set {
                extension = value;
                OnPropertyChanged();
            }
        }

        public string Content {
            get => content;
            set {
                content = value;
                OnPropertyChanged();
            }
        }

        public FormatSource Source {
            get => source;
            set {
                source = value;
                OnPropertyChanged();
            }
        }

        public bool IsDeleting {
            get => isDeleting;
            set {
                isDeleting = value;
                OnPropertyChanged();
            }
        }

        public bool IsSaving {
            get => isSaving;
            set {
                isSaving = value;
                OnPropertyChanged();
            }
        }

        public Command OnDeleteButtonClicked { get; set; }

        public Command OnSaveButtonClicked { get; set; }

        public Command OnPrintButtonClicked { get; set; }

        public string PrinterPath {
            get => $"{DriveLetter}:{Name}.{Extension}";
        }

        public string StoredPath {
            get => $"{Name}.{Extension}";
        }
    }
}
