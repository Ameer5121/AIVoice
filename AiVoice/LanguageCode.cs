using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AiVoice
{
    enum LanguageCode
    {
        [Display(Name = "English")]
        en,
        [Display(Name = "Spanish")]
        es,
        [Display(Name = "Italian")]
        it,
        [Display(Name = "Portuguese")]
        pt,
        [Display(Name = "Polish")]
        pl,
        [Display(Name = "Russian")]
        ru,
        [Display(Name = "Dutch")]
        nl,
        [Display(Name = "Indonesian")]
        id,
        [Display(Name = "Chinese")]
        ch,
        [Display(Name = "Korean")]
        ko,
        [Display(Name = "Turkish")]
        tr,
        [Display(Name = "French")]
        fr,
        [Display(Name = "Finnish")]
        fi,
        [Display(Name = "German")]
        de,
    }
}
