using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Services.ApplicationOnlineFileStorage
{
    public enum DocumentType
    {
        [Display(Name = "Выписка по счету")]
        BANK_ACCOUNT_STATEMENT = 0,
        [Display(Name = "Удостоверение личности")]
        IDENTITY_DOCUMENT = 1,
        [Display(Name = "Вид на жительство")]
        RESIDENCE_PERMIT = 2,
        [Display(Name = "Фото из документа")]
        DOCUMENT_PHOTO = 3,
        [Display(Name = "Технический паспорт")]
        TECHNICAL_PASSPORT = 4,
        [Display(Name = "Фото Машина спереди/фото авто на расстоянии 2-х метров")]
        CAR_FRONT_PHOTO = 5,
        [Display(Name = "Фото Машина справа/фото авто на расстоянии 3-х метров")]
        CAR_RIGHT_PHOTO = 6,
        [Display(Name = "Фото Машина сзади/фото авто на расстоянии 2-х метров")]
        CAR_REAR_PHOTO = 7,
        [Display(Name = "Фото Машина слева/фото авто на расстоянии 3-х метров")]
        CAR_LEFT_PHOTO = 8,
        [Display(Name = "Фото Панель приборов/фото при заведенном двигателе")]
        CAR_DASHBOARD_PHOTO = 9,
        [Display(Name = "Фото Передний ряд сидений/в кадр должны попасть 2 сидения, руль, Коробка переключения передач")]
        CAR_FRONT_SEAT_PHOTO = 10,
        [Display(Name = "Фото Задний ряд сидений/фото со спущенным подлокотником")]
        CAR_REAR_SEAT_PHOTO = 11,
        [Display(Name = "Фото VIN код/фото выбитого VIN кода на фоне Техпаспорта")]
        VIN_CODE_PHOTO = 12,
        [Display(Name = "Фото клиента на фоне машины")]
        CUSTOMER_CAR_PHOTO = 13,
        [Display(Name = "Фото наклейки с указанным VIN")]
        VIN_STICKER_PHOTO = 14,
        [Display(Name = "Фото лучший кадр биометрии")]
        BIOMETRIC_BEST_PHOTO = 15,
        [Display(Name = "Фото кредитной или дебетовой карты")]
        CREDIT_CARD_PHOTO = 16,
        [Display(Name = "Видео машины")]
        CAR_VIDEO = 17,
        [Display(Name = "Соглашение о кредитной линии")]
        CREDIT_LINE_AGREEMENT = 18,
        [Display(Name = "Договор о предоставлении микрокредита")]
        LOAN_CONTRACT = 19,
        [Display(Name = "Договор залога движимого имущества")]
        MOVABLE_PROPERTY_PLEDGE_AGREEMENT = 20,
        [Display(Name = "Согласия в Кредитные бюро")]
        CONSENTS_PKB_GKB = 21,
        [Display(Name = "Расписка")]
        RECEIPT = 22,
        [Display(Name = "Заявление (в случае выдачи кредита со страховкой)")]
        INSURED_LOAN_APPLICATION = 23,
        [Display(Name = "Заявление на расчетный счет")]
        ACCOUNT_APPLICATION = 24,
        [Display(Name = "Фото двигателя")]
        ENGINE_PHOTO = 25,
        [Display(Name = "Фото бирок на ремнях")]
        BELT_TAGS_PHOTO = 26,

    }
}
