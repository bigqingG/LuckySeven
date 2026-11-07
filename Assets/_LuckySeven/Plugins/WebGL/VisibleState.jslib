mergeInto(LibraryManager.library, {
  LuckySevenSetLocale: function(code) { if(window.luckySevenLanguage)window.luckySevenLanguage(UTF8ToString(code)); },
  LuckySevenPublish: function(json) { window.luckySevenSnapshot = JSON.parse(UTF8ToString(json)); }
});
