using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Enums
{
    public enum HardCollectionActionTypeEnum
    {
        AddAddress = 10,
        AddActualContact = 20,
        AddContact = 30,
        AddComment = 40,
        AddPhoto = 50,
        SaveAcceptanceCertificate = 60,
        MoveToParkingCar = 70,
        SendLegalCollection = 80,
        DebtRepaid = 90,
        DeleteAddress = 100,
        NotMoveToParkingCar = 110,
        AddToMyToDoList = 120,
        AddWitness = 130,
        NotLiveInAddress = 140,
        AddOnlyLog = 150,
        SendSmsCert = 160,
        VerifySmsCert = 170,
        SendSmsWitness = 180,
        VerifySmsWitness = 190,
        AddExpence = 200
    }
}
