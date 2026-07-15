using Microsoft.AspNetCore.Identity;

namespace ResumeAnalyzer.Services
{
    public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() => 
            new() { Code = nameof(DefaultError), Description = "Beklenmeyen bir hata oluştu." };

        public override IdentityError ConcurrencyFailure() => 
            new() { Code = nameof(ConcurrencyFailure), Description = "Veri tutarsızlığı algılandı." };

        public override IdentityError PasswordMismatch() => 
            new() { Code = nameof(PasswordMismatch), Description = "Mevcut şifreniz uyuşmuyor." };

        public override IdentityError InvalidToken() => 
            new() { Code = nameof(InvalidToken), Description = "Şifre sıfırlama kodu geçersiz veya süresi dolmuş." };

        public override IdentityError LoginAlreadyAssociated() => 
            new() { Code = nameof(LoginAlreadyAssociated), Description = "Bu hesapla zaten ilişkilendirilmiş bir giriş var." };

        public override IdentityError InvalidUserName(string? userName) => 
            new() { Code = nameof(InvalidUserName), Description = $"Kullanıcı adı '{userName}' geçersiz." };

        public override IdentityError InvalidEmail(string? email) => 
            new() { Code = nameof(InvalidEmail), Description = $"E-posta adresi '{email}' geçersiz." };

        public override IdentityError DuplicateUserName(string userName) => 
            new() { Code = nameof(DuplicateUserName), Description = $"'{userName}' kullanıcı adı zaten kullanımda." };

        public override IdentityError DuplicateEmail(string email) => 
            new() { Code = nameof(DuplicateEmail), Description = $"'{email}' e-posta adresi zaten kullanımda." };

        public override IdentityError PasswordTooShort(int length) => 
            new() { Code = nameof(PasswordTooShort), Description = $"Şifre en az {length} karakter uzunluğunda olmalıdır." };

        public override IdentityError PasswordRequiresNonAlphanumeric() => 
            new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Şifreniz en az bir özel karakter (alfanümerik olmayan; örn: @, !, ?, *, .) içermelidir." };

        public override IdentityError PasswordRequiresDigit() => 
            new() { Code = nameof(PasswordRequiresDigit), Description = "Şifreniz en az bir rakam ('0'-'9') içermelidir." };

        public override IdentityError PasswordRequiresLower() => 
            new() { Code = nameof(PasswordRequiresLower), Description = "Şifreniz en az bir küçük harf ('a'-'z') içermelidir." };

        public override IdentityError PasswordRequiresUpper() => 
            new() { Code = nameof(PasswordRequiresUpper), Description = "Şifreniz en az bir büyük harf ('A'-'Z') içermelidir." };
    }
}
